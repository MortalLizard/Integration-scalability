using Inventory.Database.Entities;
using Inventory.Database.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Controllers.Web;

public class BookController(IBookService bookService) : Controller
{
    // GET: Book
    public async Task<IActionResult> Index()
    {
        var books = await bookService.GetAllAsync();

        return View(books);
    }

    // GET: Book/Details/5
    public async Task<IActionResult> Details(Guid id)
    {
        var book = await bookService.GetByIdAsync(id);

        if (book == null) return NotFound();

        return View(book);
    }

    // GET: Book/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Book/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Book book)
    {
        if (!ModelState.IsValid) return View(book);

        await bookService.CreateAsync(book);

        return RedirectToAction(nameof(Index));
    }

    // GET: Book/Edit/5
    public async Task<IActionResult> Edit(Guid id)
    {
        var book = await bookService.GetByIdAsync(id);

        if (book == null) return NotFound();

        return View(book);
    }

    // POST: Book/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Book book)
    {
        if (id != book.Id) return NotFound();

        if (!ModelState.IsValid) return View(book);

        await bookService.UpdateAsync(book);

        return RedirectToAction(nameof(Index));
    }

    // GET: Book/Delete/5
    public async Task<IActionResult> Delete(Guid id)
    {
        var book = await bookService.GetByIdAsync(id);

        if (book == null) return NotFound();

        return View(book);
    }

    // POST: Book/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await bookService.DeleteAsync(id);

        return RedirectToAction(nameof(Index));
    }
}
