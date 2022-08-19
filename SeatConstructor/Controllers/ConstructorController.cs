using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SeatConstructor.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SeatConstructor.Models.DB;

namespace SeatConstructor.Controllers
{
    public class ConstructorController : Controller
    {

        private ApplicationContext context;

        public ConstructorController(ApplicationContext context)
        {
            this.context = context;
        }
        //----------------------------------------------------------------

        [HttpGet]
        public IActionResult Main()
        {
            return View(context.Seats.ToArray());
        }

        public JsonResult GetAvaliableMods(int seatId)
        {
            return Json(context.ModsForSeats.Where(x => x.SeatId == seatId).OrderBy(x => x.Type.Name).ToArray());
        }

        public string GetSummaryCode(int seatId, IEnumerable<ModForSeat> mods)
        {
            Seat seat = context.Seats.Find(seatId);
            if (seat == null) return null;

            var sortedTypes = context.ModificationTypes.OrderBy(x => x.PositionInSummaryCode).ToList();

            string summaryCode = seat.Code;
            foreach (var type in sortedTypes)
            {
                var modOfType = mods.FirstOrDefault(x => x.TypeId == type.Id);
                summaryCode += modOfType == null ? "0" : modOfType.Code;
            }
            summaryCode += seat.Postfix;

            return summaryCode;
        }

        public async void SendOrderRequest(string summaryCode, string contactName, string contactEmail, string contactTel)
        {
            if (string.IsNullOrEmpty(summaryCode)) return;

            string[] validationErrors = (string[])ValidateContacts(contactName, contactEmail, contactTel).Value;

            bool valid = validationErrors.Count(x => string.IsNullOrEmpty(x)) == validationErrors.Length;

            if (valid)
            {
                Order order = new Order() { SummaryCode = summaryCode, ContactEmail = contactEmail, ContactName = contactName, ContactTel = contactTel };

                context.Orders.Add(order);
                context.SaveChanges();
                await EmailService.SendEmailAsync(
                    "egor.demyanenko2000@mail.ru", // Почта на которую будут лететь заявки
                    "Заявка от конструктора", // Тема письма

                    "Артикул - " + summaryCode + // Текст письма
                    "<br><br> Контакты:" +
                    "<br> Имя - " + contactName +
                    "<br> Телефон - " + contactTel +
                    "<br> Email - " + contactEmail
                    );
            }
        }


        public JsonResult ValidateContacts(string name, string email, string tel)
        {
            string[] errors = new string[3];

            bool isTelValid = !string.IsNullOrEmpty(tel) && !tel.EndsWith('_');
            bool isMailValid = EmailService.IsValidEmail(email);

            if (string.IsNullOrEmpty(name)) errors[0] = "Пожалуйста укажите имя";

            if (!isMailValid) errors[1] = "Поле 'E-Mail' должно быть действительным электронным адресом";
            if (string.IsNullOrEmpty(email) && isTelValid) errors[1] = null;

            if (!isTelValid) errors[2] = "Некорректно введен номер телефона";
            if (string.IsNullOrEmpty(tel) && isMailValid) errors[2] = null;

            if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(tel))
            {
                errors[1] = "Пожалуйста введите E-Mail"; errors[2] = "Пожалуйста введите номер телефона";
            }

            if (!isTelValid && string.IsNullOrEmpty(email))
            {
                errors[1] = "Пожалуйста введите E-Mail";
            }

            if (!isMailValid && string.IsNullOrEmpty(tel))
            {
                errors[2] = "Пожалуйста введите номер телефона";
            }

            return Json(errors);
        }


        //----------------------------------------------------------------

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
