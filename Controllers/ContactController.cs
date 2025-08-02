using Microsoft.AspNetCore.Mvc;
using BrettGravesPortfolio.Models;
using BrettGravesPortfolio.Services;

namespace BrettGravesPortfolio.Controllers;

public class ContactController : Controller
{
    private readonly IEmailSender _email;
    private readonly IConfiguration _cfg;

    public ContactController(IEmailSender email, IConfiguration cfg)
    {
        _email = email;
        _cfg = cfg;
    }

    [HttpGet]
    public IActionResult Index() => View(new ContactForm());

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> Index(ContactForm form, CancellationToken ct)
    {
        // Honeypot – bots fill it; humans leave blank
        if (!string.IsNullOrEmpty(form.Website))
        {
            TempData["Sent"] = true;
            return RedirectToAction(nameof(Thanks));
        }

        if (!ModelState.IsValid) return View(form);

        var to = _cfg["CONTACT_TO"] ?? "gravesbr10@gmail.com";
        var from = _cfg["CONTACT_FROM"] ?? "gravesbr10@gmail.com"; // must match Single Sender unless you auth a domain

        var body = $@"Name: {form.Name}
Email: {form.Email}
Subject: {form.Subject}

{form.Message}";

        try
        {
            await _email.SendAsync(to, from, $"Portfolio contact: {form.Subject}", body, ct);
            TempData["Sent"] = true;
            return RedirectToAction(nameof(Thanks));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            ModelState.AddModelError("", "Sorry—sending failed. Please try again later.");
            return View(form);
        }
    }

    [HttpGet]
    public IActionResult Thanks()
    {
        if (TempData["Sent"] is true) return View();
        return RedirectToAction("Thanks", "Contact");
    }
}