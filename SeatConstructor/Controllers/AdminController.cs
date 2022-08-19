using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SeatConstructor.Models;
using SeatConstructor.Models.DB;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SeatConstructor.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private string rootPath => environment.WebRootPath;

        private ApplicationContext context;
        private IWebHostEnvironment environment;

        public AdminController(ApplicationContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }

        //----------------------------------------------------------------

        [HttpGet]
        public ActionResult Main()
        {
            return View(context.Seats);
        }


        #region Seats
        [HttpGet]
        public ActionResult EditSeat(int id)
        {
            Seat seat = context.Seats.Find(id);
            if (seat == null) return NotFound();

            ViewBag.ModTypes = context.ModificationTypes;
            return View(seat);
        }

        public void ApplyChanges(int seatId, string newName, string newCode, string newPostfix)
        {
            Seat seat = context.Seats.Find(seatId);
            if (seat == null) return;

            if (string.IsNullOrEmpty(newName) || string.IsNullOrEmpty(newCode)) return;
            Seat seatByNewName = context.Seats.FirstOrDefault(x => x.Name == newName);
            //Seat seatByNewCode = context.Seats.FirstOrDefault(x => x.Code == newCode);

            if (seatByNewName != null && seatByNewName.Id != seatId) return;
            //if (seatByNewCode != null && seatByNewCode.Id != seatId) return;

            if (newName != seat.Name)
            {
                string file3dName = Path.GetFileName(seat.PathTo3DModel);
                string fileImgName = Path.GetFileName(seat.PathToImage);

                seat.PathTo3DModel = "." + @"/3D/Сиденья/" + newName + "/" + file3dName;
                seat.PathToImage = "." + @"/3D/Сиденья/" + newName + "/" + fileImgName;

                ModForSeat[] mods = context.ModsForSeats.Where(x => x.SeatId == seatId).ToArray();
                foreach (var mod in mods)
                {
                    string modFileName = Path.GetFileName(mod.PathTo3DModel);
                    mod.PathTo3DModel = "." + @"/3D/Сиденья/" + newName + "/" + mod.Type.Name + "/" + modFileName;
                }

                Directory.Move(rootPath + @"/3D/Сиденья/" + seat.Name, rootPath + @"/3D/Сиденья/" + newName);
            }

            seat.Name = newName;
            seat.Code = newCode;
            seat.Postfix = newPostfix;


            context.SaveChanges();
        }

        public JsonResult RefreshSeats()
        {
            return Json(context.Seats.ToArray());
        }

        public async Task AddSeat(string name, string code, string postfix, IFormFile file3d, IFormFile fileImg)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(code) || file3d == null || fileImg == null) return;
            if (context.Seats.FirstOrDefault(x => x.Name == name) != null) return;
            //if (context.Seats.FirstOrDefault(x => x.Code == code) != null) return;
           
            if (!Directory.Exists(rootPath + @"/3D/Сиденья/" + name + "/")) Directory.CreateDirectory(rootPath + @"/3D/Сиденья/" + name + "/");
            foreach (ModificationType type in context.ModificationTypes)
            {
                if (!Directory.Exists(rootPath + @"/3D/Сиденья/" + name + "/" + type.Name)) Directory.CreateDirectory(rootPath + @"/3D/Сиденья/" + name + "/" + type.Name);
            }

            bool uploaded = await FileUploader.TryUploadFile(file3d, rootPath, @"/3D/Сиденья/" + name + "/");
            uploaded |= await FileUploader.TryUploadFile(fileImg, rootPath, @"/3D/Сиденья/" + name + "/");

            if (!uploaded) return;

            Seat newSeat = new Seat()
            {
                Name = name,
                Code = code,
                Postfix = postfix,
                PathTo3DModel = "." + @"/3D/Сиденья/" + name + "/" + file3d.FileName,
                PathToImage = "." + @"/3D/Сиденья/" + name + "/" + fileImg.FileName,

            };
            context.Seats.Add(newSeat);
            context.SaveChanges();
        }

        public void RemoveSeat(int seatId)
        {
            var seat = context.Seats.Find(seatId);
            if (seat != null)
            {
                //FileUploader.Remove(rootPath + seat.PathTo3DModel);
                context.Seats.Remove(seat);
                context.SaveChanges();

                if (Directory.Exists(rootPath + @"/3D/Сиденья/" + seat.Name)) Directory.Delete(rootPath + @"/3D/Сиденья/" + seat.Name, true);
            }
        }
        #endregion

        #region ModTypes

        [HttpGet]
        public ActionResult ModTypes()
        {
            return View();
        }

        public JsonResult RefreshModTypes()
        {
            return Json(context.ModificationTypes.OrderBy(x => x.PositionInSummaryCode).ToArray());
        }

        public void MoveTypeUp(int modTypeId)
        {
            ModificationType type = context.ModificationTypes.Find(modTypeId);
            if (type == null) return;

            var sortedTypes = context.ModificationTypes.OrderBy(x => x.PositionInSummaryCode).ToList();

            if (type.Id == sortedTypes[0].Id) return;

            int index = sortedTypes.IndexOf(type);

            int buffer = type.PositionInSummaryCode;
            type.PositionInSummaryCode = sortedTypes[index - 1].PositionInSummaryCode;
            sortedTypes[index - 1].PositionInSummaryCode = buffer;

            context.SaveChanges();
        }

        public void MoveTypeDown(int modTypeId)
        {
            ModificationType type = context.ModificationTypes.Find(modTypeId);
            if (type == null) return;

            var sortedTypes = context.ModificationTypes.OrderBy(x => x.PositionInSummaryCode).ToList();

            if (type.Id == sortedTypes[sortedTypes.Count - 1].Id) return;

            int index = sortedTypes.IndexOf(type);

            int buffer = type.PositionInSummaryCode;
            type.PositionInSummaryCode = sortedTypes[index + 1].PositionInSummaryCode;
            sortedTypes[index + 1].PositionInSummaryCode = buffer;

            context.SaveChanges();
        }

        public void AddModType(string name)
        {
            if (string.IsNullOrEmpty(name)) return;

            if (context.ModificationTypes.FirstOrDefault(x => x.Name == name) != null) return;


            foreach (var seat in context.Seats)
            {
                Directory.CreateDirectory(rootPath + @"/3D/Сиденья/" + seat.Name + "/" + name);
            }
            var nextPositionInSummaryCode = 0;

            if (context.ModificationTypes.Count() != 0)
            {
                nextPositionInSummaryCode = context.ModificationTypes.Max(x => x.PositionInSummaryCode) + 1;
            }
            ModificationType type = new ModificationType() { Name = name, PositionInSummaryCode = nextPositionInSummaryCode };
            context.ModificationTypes.Add(type);
            context.SaveChanges();
        }
        public void EditModType(int modTypeId, string name)
        {
            if (string.IsNullOrEmpty(name)) return;

            ModificationType editType = context.ModificationTypes.Find(modTypeId);
            if (editType == null) return;

            ModificationType type = context.ModificationTypes.FirstOrDefault(x => x.Name == name);
            if (type != null) return;

            if (name[name.Length - 1] == ' ') name = name.Remove(name.Length - 1);

            string prevName = editType.Name;
            editType.Name = name;

            context.ModificationTypes.Update(editType);

            foreach (var seat in context.Seats)
            {
                Directory.Move(rootPath + @"/3D/Сиденья/" + seat.Name + "/" + prevName, rootPath + @"/3D/Сиденья/" + seat.Name + "/" + name);
            }

            ModForSeat[] mods = context.ModsForSeats.Where(x => x.TypeId == modTypeId).ToArray();
            foreach (var mod in mods)
            {
                string modFileName = Path.GetFileName(mod.PathTo3DModel);
                mod.PathTo3DModel = "." + @"/3D/Сиденья/" + mod.Seat.Name + "/" + name + "/" + modFileName;
            }

            context.SaveChanges();
        }
        public void RemoveModType(int modTypeId)
        {
            var type = context.ModificationTypes.Find(modTypeId);
            if (type == null) return;

            foreach (var seat in context.Seats)
            {
                if (Directory.Exists(rootPath + @"/3D/Сиденья/" + seat.Name + "/" + type.Name))
                    Directory.Delete(rootPath + @"/3D/Сиденья/" + seat.Name + "/" + type.Name, true);
            }

            var typesAfterRemoving = context.ModificationTypes.Where(x => x.PositionInSummaryCode > type.PositionInSummaryCode);
            foreach (var t in typesAfterRemoving)
            {
                t.PositionInSummaryCode -= 1;
            }

            context.ModificationTypes.Remove(type);
            context.SaveChanges();
        }
        #endregion

        #region Mods
        public JsonResult RefreshMods(int seatId)
        {
            return Json(context.ModsForSeats.Where(x => x.SeatId == seatId).ToArray());
        }

        public async Task AddMod(int seatId, int typeId, string name, string code, string description, IFormFile file3d)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(code) || file3d == null) return;

            var existingModWithSameNameAndType = context.ModsForSeats.FirstOrDefault(x => x.Name == name && x.TypeId == typeId);
            if (existingModWithSameNameAndType != null && existingModWithSameNameAndType.SeatId == seatId) return;

            Seat seat = context.Seats.Find(seatId);
            if (seat == null) return;

            ModificationType type = context.ModificationTypes.Find(typeId);
            if (type == null) return;

            bool uploaded = await FileUploader.TryUploadFile(file3d, rootPath, @"/3D/Сиденья/" + seat.Name + @"/" + type.Name + @"/", "");
            if (!uploaded) return;

            ModForSeat mod = new ModForSeat()
            {
                Name = name,
                Code = code,
                Description = description,
                SeatId = seatId,
                PathTo3DModel = "." + @"/3D/Сиденья/" + seat.Name + @"/" + type.Name + @"/" + file3d.FileName,
                TypeId = typeId,
            };

            context.ModsForSeats.Add(mod);
            context.SaveChanges();
        }

        public void RemoveMod(int modId)
        {
            var mod = context.ModsForSeats.Find(modId);
            if (mod != null)
            {
                FileUploader.Remove(rootPath + mod.PathTo3DModel); // potential bug
                context.ModsForSeats.Remove(mod);
                context.SaveChanges();
            }
        }

        public void EditModName(int modId, int seatId, string name)
        {
            var mod = context.ModsForSeats.Find(modId);
            if (mod != null)
            {
                if (string.IsNullOrEmpty(name)) return;
                var existingModWithSameNameAndType = context.ModsForSeats.FirstOrDefault(x => x.Name == name && x.TypeId == mod.TypeId);
                if (existingModWithSameNameAndType != null && existingModWithSameNameAndType.SeatId == seatId) return;

                mod.Name = name;
                context.SaveChanges();
            }
        }

        public void EditModCode(int modId, string code)
        {
            var mod = context.ModsForSeats.Find(modId);
            if (mod != null)
            {
                if (string.IsNullOrEmpty(code)) return;

                mod.Code = code;
                context.SaveChanges();
            }
        }
        public void EditModMirror(int modId, bool mirror)
        {
            var mod = context.ModsForSeats.Find(modId);
            if (mod != null)
            {
                mod.Mirror = mirror;
                context.SaveChanges();
            }
        }

        public void EditModDescription(int modId, string description)
        {
            var mod = context.ModsForSeats.Find(modId);
            if (mod != null)
            {
                mod.Description = description;
                context.SaveChanges();
            }
        }
        #endregion

        #region ModPlacement
        public void SaveModPlacement(int modId, string xOffset, string yOffset, string zOffset)
        {
            ModForSeat mod = context.ModsForSeats.Find(modId);
            if (mod != null)
            {
                mod.OffsetX = /*float.Parse(xOffset.Replace('.', ','))*/xOffset;
                mod.OffsetY = /*float.Parse(yOffset.Replace('.', ','))*/yOffset;
                mod.OffsetZ = /*float.Parse(zOffset.Replace('.', ','))*/zOffset;
                context.SaveChanges();
            }
        }

        public void SetDefaultModPlacement(int modId)
        {
            ModForSeat mod = context.ModsForSeats.Find(modId);
            if (mod != null)
            {
                mod.OffsetX = "0";
                mod.OffsetY = "0";
                mod.OffsetZ = "0";
                context.SaveChanges();
            }
        }

        public string[] CancelModPlacement(int modId)
        {
            ModForSeat mod = context.ModsForSeats.Find(modId);
            if (mod != null)
            {
                string[] current = new string[] { mod.OffsetX, mod.OffsetY, mod.OffsetZ };
                return current;
            }
            return null;
        }

        #endregion
    }


    // Фарш, Луковицу (1) большую мелко порезать, кинуть в фарш, промыть рис, засыпать в фарш, посолить 1чл поперчить 0.5чл

    // перцы помыть, вырезать шляпку и аккуратно внутринности (особенно перегородки),
    // чайной ложкой накладывать фарш (не полные, поровну в каждый перец)
    // кастрюлю по больше залить водой и полчайнойложки соли туда, можно чтобы верхние перцы выступали над водой потом на медленный как закипят

    // сковородку подсолнечное масло, луковицу (1), морковку (оранжевой теркой) (0.5-1)
    // помидорку помыть и порезать маленькими кубиками (соль перец по вкусу) 
    // 5-7 минут на сковороде, затем в кастрюлю

    // 40 минут в кипящей кастрюле на медленном огне крышкой не закрывать

    // укроп, петрушку, зеленый лук порезать и забросить на 3 минуты туда же 
}
