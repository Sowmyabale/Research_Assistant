using Microsoft.AspNetCore.Mvc;
using Research_Assistant.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Xobject;
using iText.Kernel.Pdf.Tagging;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class PaperController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public PaperController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // List all uploaded papers
    [HttpGet]
    public async Task<IActionResult> Index()
    {
         var papers = await _context.Papers
                                    .GroupBy(p => new { p.Title, p.Author })
                                    .Select(g => g.First())
                                    .ToListAsync();
        return View(papers);
    }

    // Show Upload Form
    [HttpGet]
    public IActionResult Upload()
    {
        return View();
    }

    // Handle File Upload
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "Please upload a valid PDF file.");
            return View();
        }

        // Save file to wwwroot/uploads
        string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var filePath = Path.Combine(uploadsFolder, file.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Extract metadata
        string title = "Unknown";
        string author = "Unknown";
        string extractedText = "";
        DateTime? publicationDate = null;

        using (PdfReader reader = new PdfReader(filePath))
        {
            PdfDocument pdfDoc = new PdfDocument(reader);
            var docInfo = pdfDoc.GetDocumentInfo();

            title = pdfDoc.GetDocumentInfo().GetTitle() ?? "Unknown Title";
            author = pdfDoc.GetDocumentInfo().GetAuthor() ?? "Unknown Author";

            // Extract text
            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
            extractedText += PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i)) + " ";
            }

            string dateString = docInfo.GetMoreInfo("CreationDate");
            if (!string.IsNullOrEmpty(dateString))
            {
            publicationDate = ExtractPublicationDate(dateString);
            }
        }

        // Extract text
        extractedText = ExtractTextFromPdf(filePath);

        // Check if the paper already exists in the database based on Title and Author
        var existingPaper = await _context.Papers
                    .FirstOrDefaultAsync(p => p.Title == title && p.Author == author);

        if (existingPaper != null)
        {
            ModelState.AddModelError("", "This paper has already been uploaded.");
            return View(); // Return to the upload view with an error message
        }

        // Get all existing document texts for TF-IDF
        var allDocuments = await _context.Papers.Select(p => p.ExtractedText).ToListAsync();
        allDocuments.Add(extractedText); // Include the new document

        // Compute keywords using TF-IDF
        //string extractedKeywords = ExtractKeywordsUsingTFIDF(extractedText, allDocuments);
        string extractedKeywords = ExtractKeywordsUsingTFIDF(extractedText, allDocuments, author);


        // Save to database
        var paper = new Paper
        {
            Title = title,
            Author = author,
            Keywords = extractedKeywords,
            PublicationDate = publicationDate,
            FilePath = "/uploads/" + file.FileName,
            ExtractedText = extractedText
        };

        _context.Papers.Add(paper);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index"); // Redirect to list of papers after upload
    }
    // Helper function to parse the PDF metadata date
    private DateTime? ExtractPublicationDate(string pdfDate)
    {
    var match = Regex.Match(pdfDate, @"D:(\d{4})(\d{2})?(\d{2})?");
    if (match.Success)
    {
        int year = int.Parse(match.Groups[1].Value);
        int month = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 1;
        int day = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 1;
        return new DateTime(year, month, day);
    }
    return null;
    }

/*private string ExtractTextFromPdf(string filePath)
{
    StringBuilder text = new StringBuilder();

    using (PdfReader reader = new PdfReader(filePath))
    {
        PdfDocument pdfDoc = new PdfDocument(reader);
        for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
        {
            text.Append(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i)));
        }
    }
    return text.ToString();
}*/
private string ExtractTextFromPdf(string filePath)
{
    StringBuilder text = new StringBuilder();
    bool abstractFound = false;
    bool conclusionFound = false;

    using (PdfReader reader = new PdfReader(filePath))
    {
        PdfDocument pdfDoc = new PdfDocument(reader);
        for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
        {
            string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i));

            if (!abstractFound)
            {
                // Look for 'Abstract' keyword (case-insensitive)
                Match match = Regex.Match(pageText, @"(?i)\babstract\b");
                if (match.Success)
                {
                    abstractFound = true;
                    text.Append(pageText.Substring(match.Index)); // Start from Abstract
                }
            }
            else if (!conclusionFound)
            {
                // Stop at 'Conclusion' or similar words
                Match match = Regex.Match(pageText, @"(?i)\b(conclusion|summary|final remarks|closing remarks)\b");
                if (match.Success)
                {
                    conclusionFound = true;
                    text.Append(pageText.Substring(0, match.Index)); // Stop at Conclusion
                    break; // Exit loop after finding Conclusion
                }
                else
                {
                    text.Append(pageText);
                }
            }
        }
    }

    return text.ToString();
}

private string ExtractKeywordsUsingTFIDF(string text, List<string> allDocuments, string author, int topN = 10)
{
    var words = Regex.Matches(text.ToLower(), @"\b[a-zA-Z]{9,}\b")
                     .Select(m => m.Value)
                     .ToList();

    var stopwords = new HashSet<string>
    {
        "university", "college", "institute", "department", "professor",
        "research", "science", "engineering", "technology",
        "city", "country", "school", "faculty",
        "john", "michael", "david", "james", "robert", "mary", "jennifer", // Common first names
        "usa", "europe", "india", "germany", "canada", "france", "china", // Countries
        "ieee", "springer", "elsevier", "arxiv", "wiley" // Common paper sources
    };

    words = words.Where(word => !stopwords.Contains(word)).ToList();
    
    var tf = words.GroupBy(w => w)
                  .ToDictionary(g => g.Key, g => (double)g.Count() / words.Count); // Term Frequency

    var idf = new Dictionary<string, double>();
    int totalDocs = allDocuments.Count;

    foreach (var word in tf.Keys)
    {
        int docsWithWord = allDocuments.Count(d => d.Contains(word));
        idf[word] = Math.Log((double)totalDocs / (1 + docsWithWord)); // Avoid division by zero
    }

    var tfidf = tf.ToDictionary(kvp => kvp.Key, kvp => kvp.Value * idf[kvp.Key]); // Compute TF-IDF

    return string.Join(", ", tfidf.OrderByDescending(kvp => kvp.Value)
                                   .Take(topN)
                                   .Select(kvp => kvp.Key));
}

[HttpGet]
public async Task<IActionResult> Search(string query)
{
    if (string.IsNullOrWhiteSpace(query))
    {
        return View("Index", await _context.Papers.ToListAsync()); // Show all if no search query
    }

    // Fetch all papers
    var papers = await _context.Papers.ToListAsync();

    // Check if the query matches any author's name
    var authorMatches = papers.Where(p => p.Author != null && p.Author.ToLower().Contains(query.ToLower())).ToList();

    // Compute TF-IDF scores and rank results
    var rankedResults = RankByTfIdf(query, papers)
                        .OrderByDescending(p => p.Score)
                        .Select(p => p.Paper)
                        .ToList();

    // Combine results: Give priority to author matches
    var finalResults = authorMatches.Concat(rankedResults)
                                     .GroupBy(p => p.Id) // Avoid duplicates by Paper Id
                                     .Select(g => g.First())
                                     .ToList();

    return View("Index", finalResults);
}

private List<(Paper Paper, double Score)> RankByTfIdf(string query, List<Paper> papers)
{
    var queryWords = query.ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries);

    var allWords = papers.SelectMany(p => p.Keywords.Split(", ")).Distinct().ToList();
    var termIndex = allWords.Select((word, index) => new { word, index })
                            .ToDictionary(x => x.word, x => x.index);

    int numDocs = papers.Count;
    int numTerms = allWords.Count;
    var tfMatrix = DenseMatrix.OfArray(new double[numDocs, numTerms]);

    for (int i = 0; i < papers.Count; i++)
    {
        var words = papers[i].Keywords.Split(", ");
        foreach (var word in words)
        {
            if (termIndex.ContainsKey(word))
                tfMatrix[i, termIndex[word]] += 1;
        }
    }

    var idf = DenseVector.OfEnumerable(allWords.Select(word =>
    {
        int docCount = papers.Count(p => p.Keywords.Contains(word));
        return Math.Log((double)numDocs / (1 + docCount));
    }));

    var tfidfMatrix = tfMatrix.PointwiseMultiply(DenseMatrix.OfRows(numDocs, numTerms, Enumerable.Repeat(idf.ToArray(), numDocs).Select(x => x.AsEnumerable())));

    var queryVector = DenseVector.OfEnumerable(allWords.Select(word =>
        queryWords.Contains(word) ? 1.0 : 0.0));

    var results = new List<(Paper, double)>();
    for (int i = 0; i < papers.Count; i++)
    {
        double score = CosineSimilarity(tfidfMatrix.Row(i), queryVector);
        results.Add((papers[i], score));
    }

    return results;
}

// Cosine Similarity Calculation
private double CosineSimilarity(Vector<double> v1, Vector<double> v2)
{
    double dotProduct = v1.DotProduct(v2);
    double magnitude = v1.L2Norm() * v2.L2Norm();
    return magnitude == 0 ? 0 : dotProduct / magnitude;
}


}
