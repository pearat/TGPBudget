using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using TgpBudget.Models;

namespace TgpBudget.Controllers
{
    [RequireHttps]
    public class InvitationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Invitations
        //public ActionResult Index()
        //{
        //    var invitations = db.Invitations.Include(i => i.Household);
        //    return View(invitations.ToList());
        //}

        // GET: Invitations/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invitation invitation = db.Invitations.Find(id);
            if (invitation == null)
            {
                return HttpNotFound();
            }
            return View(invitation);
        }

        // Helper function: GetUniqueKey
        private string GetUniqueKey(int maxSize)
        {
            char[] chars = new char[62];
            chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }


        // GET: Invitations/Create
        public ActionResult Create()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            if (user == null)
            {
                return RedirectToAction("JoinCreate","Households");
            }
            var invite = new Invitation();
            var HhId = (int)user.HouseholdId;
            invite.HouseholdId = HhId;
            invite.HouseholdName = user.Household.Name;
            invite.IssuedBy = user.Email;

            return View(invite);
        }

        // POST: Invitations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "HouseholdId,HouseholdName,IssuedBy,GuestEmail")] Invitation invitation)
        {
            var INVITATION_CODE_LENGTH = 12;
            var EXPIRATION_DAYS = 7;
            var EXPIRATION_HOURS = 24;

            if (ModelState.IsValid)
            {
                invitation.IssuedOn = System.DateTimeOffset.Now;
                invitation.InvalidAfter = invitation.IssuedOn.AddHours(EXPIRATION_HOURS); //.AddDays(EXPIRATION_DAYS);

                invitation.InvitationCode = GetUniqueKey(INVITATION_CODE_LENGTH);
                db.Invitations.Add(invitation);
                db.SaveChanges();
                // vvvvvvvvv end send Email vvvvvvvvv

                var callbackRegisterUrl = Url.Action("Register", "Account", new { code = invitation.InvitationCode }, protocol: Request.Url.Scheme);
                var manualRegisterUrl = Url.Action("Register", "Account", new { code = "" }, protocol: Request.Url.Scheme);
                var callbackLoginUrl = Url.Action("Login", "Account", new { code = invitation.InvitationCode }, protocol: Request.Url.Scheme);
                var manualLoginUrl = Url.Action("Login", "Account", new { code = "" }, protocol: Request.Url.Scheme);
                //await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                var SendTo = invitation.GuestEmail;
                var es = new EmailService();
                var msg = new IdentityMessage
                {
                    Subject = "Invitation to join TGP-Budget App",
                    Destination = SendTo,
                    Body = "On " + invitation.IssuedOn.DateTime.ToLongDateString() + ", " + invitation.IssuedBy +
                    " sent you an invitation to join the " + invitation.HouseholdName + " house hold.<br />" +
                    "Please register at  within the next " + EXPIRATION_DAYS + " days. <br />" +
                    "To REGISTER with TGP-Budget and JOIN this household  by clicking <a href=\"" + callbackRegisterUrl + "\">here.</a><br /><br />" +
                    "To login manually, navigate to this address: < a href =\"" + manualRegisterUrl + "\">here</a>" +
                    "and enter the following household code: <b>" + invitation.InvitationCode + "</b> <br /><br /><br />" +

                    "(Note: if you have already visited registered, then" +
                    "To LOGIN  by clicking <a href=\"" + callbackLoginUrl + "\">here.</a><br /><br />" +
                    "To login manually, navigate to this address: < a href =\"" + manualLoginUrl + "\">here</a>" +
                    "and enter the following household code: <b>" + invitation.InvitationCode + "</b> )."
                };
                es.SendAsync(msg);

                // ^^^^^^^^^ end send Email ^^^^^^^^^
                return RedirectToAction("Index", "Dashboard", "Home");
            }

            //ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name", invitation.HouseholdId);
            return View(invitation);
        }

        // GET: Invitations/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invitation invitation = db.Invitations.Find(id);
            if (invitation == null)
            {
                return HttpNotFound();
            }
            ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name", invitation.HouseholdId);
            return View(invitation);
        }

        // POST: Invitations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,HouseholdId,UserEmail,InvitationCode,IssuedOn,InvalidAfter")] Invitation invitation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(invitation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name", invitation.HouseholdId);
            return View(invitation);
        }

        // GET: Invitations/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invitation invitation = db.Invitations.Find(id);
            if (invitation == null)
            {
                return HttpNotFound();
            }
            return View(invitation);
        }

        // POST: Invitations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Invitation invitation = db.Invitations.Find(id);
            db.Invitations.Remove(invitation);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}

