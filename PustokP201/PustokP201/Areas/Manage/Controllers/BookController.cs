using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PustokP201.Helper;
using PustokP201.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PustokP201.Areas.Manage.Controllers
{
    [Area("manage")]
    public class BookController : Controller
    {
        private readonly PustokContext _context;
        private readonly IWebHostEnvironment _env;

        public BookController(PustokContext context,IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public IActionResult Index(int page=1)
        {

            ViewBag.SelectedPage = page;
            ViewBag.TotalPage = (int)Math.Ceiling(_context.Books.Count() / 4d);

            return View(_context.Books.Include(x=>x.Genre).Include(x=>x.Author).Include(x=>x.BookImages).Skip((page-1)*4).Take(4).ToList());
        }

        public IActionResult Create()
        {
            ViewBag.Authors = _context.Authors.ToList();
            ViewBag.Genres = _context.Genres.ToList();
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Book book)
        {
            ViewBag.Authors = _context.Authors.ToList();
            ViewBag.Genres = _context.Genres.ToList();


            if (!ModelState.IsValid) return View();

            if (!_context.Authors.Any(x => x.Id == book.AuthorId))
            {
                ModelState.AddModelError("AuthorId", "Author not found");
                return View();
            }

            if (!_context.Genres.Any(x => x.Id == book.GenreId))
            {
                ModelState.AddModelError("GenreId", "Genre not found");
                return View();
            }

            if (book.PosterFile == null)
            {
                ModelState.AddModelError("PosterFile", "Poster file is required");
                return View();
            }
            else
            {
                if (book.PosterFile.Length > 2097152)
                {
                    ModelState.AddModelError("PosterFile", "PosterFile max size is 2MB");
                    return View();
                }
                if (book.PosterFile.ContentType != "image/jpeg" && book.PosterFile.ContentType != "image/png")
                {
                    ModelState.AddModelError("PosterFile", "ContentType must be image/jpeg or image/png");
                    return View();
                }

                BookImage poster = new BookImage
                {
                    Image = FileManager.Save(_env.WebRootPath, "uploads/books", book.PosterFile),
                    Book = book,
                    PosterStatus = true
                };

                _context.BookImages.Add(poster);
            }

            if (book.HoverPosterFile == null)
            {
                ModelState.AddModelError("HoverPosterFile", "HoverPosterFile file is required");
                return View();
            }
            else
            {
                if (book.HoverPosterFile.Length > 2097152)
                {
                    ModelState.AddModelError("HoverPosterFile", "HoverPosterFile max size is 2MB");
                    return View();
                }
                if (book.HoverPosterFile.ContentType != "image/jpeg" && book.HoverPosterFile.ContentType != "image/png")
                {
                    ModelState.AddModelError("HoverPosterFile", "ContentType must be image/jpeg or image/png");
                    return View();
                }

                BookImage poster = new BookImage
                {
                    Image = FileManager.Save(_env.WebRootPath, "uploads/books", book.HoverPosterFile),
                    Book = book,
                    PosterStatus = false
                };

                _context.BookImages.Add(poster);
            }



            if (book.ImageFiles != null)
            {
                foreach (var item in book.ImageFiles)
                {
                    if (item.Length > 2097152)
                    {
                        ModelState.AddModelError("ImageFiles", "ImageFile max size is 2MB");
                        return View();
                    }
                    if (item.ContentType != "image/jpeg" && item.ContentType != "image/png")
                    {
                        ModelState.AddModelError("ImageFiles", "ContentType must be image/jpeg or image/png");
                        return View();
                    }
                    

                    BookImage bookImage = new BookImage
                    {
                        Book = book,
                        Image = FileManager.Save(_env.WebRootPath, "uploads/books", item)
                    };

                    _context.BookImages.Add(bookImage);
                }
            }

            _context.Books.Add(book);
            _context.SaveChanges();

            return RedirectToAction("index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            Book book = await _context.Books.Include(x=>x.BookImages).FirstOrDefaultAsync(x => x.Id == id);
            if (book == null) return NotFound();

            ViewBag.Authors = _context.Authors.ToList();
            ViewBag.Genres = _context.Genres.ToList();

            return View(book);
        }




    }
}
