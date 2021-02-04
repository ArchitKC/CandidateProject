using CandidateProject.EntityModels;
using CandidateProject.Utils;
using CandidateProject.ViewModels;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace CandidateProject.Controllers
{
    public class CartonController : Controller
    {
        private CartonContext db = new CartonContext();

        // GET: Carton
        public ActionResult Index()
        {
            var cartons = db.Cartons
                .Select(c => new CartonViewModel()
                {
                    Id = c.Id,
                    CartonNumber = c.CartonNumber,
                    count = c.CartonDetails.Count()
                })
                .ToList();
            
            return View(cartons);
        }

        // GET: Carton/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var carton = db.Cartons
                .Where(c => c.Id == id)
                .Select(c => new CartonViewModel()
                {
                    Id = c.Id,
                    CartonNumber = c.CartonNumber
                })
                .SingleOrDefault();
            if (carton == null)
            {
                return HttpNotFound();
            }
            return View(carton);
        }

        // GET: Carton/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Carton/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CartonNumber")] Carton carton)
        {
            if (ModelState.IsValid)
            {
                db.Cartons.Add(carton);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(carton);
        }

        // GET: Carton/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var carton = db.Cartons
                .Where(c => c.Id == id)
                .Select(c => new CartonViewModel()
                {
                    Id = c.Id,
                    CartonNumber = c.CartonNumber
                })
                .SingleOrDefault();
            if (carton == null)
            {
                return HttpNotFound();
            }
            return View(carton);
        }

        // POST: Carton/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CartonNumber")] CartonViewModel cartonViewModel)
        {
            if (ModelState.IsValid)
            {
                var carton = db.Cartons.Find(cartonViewModel.Id);
                carton.CartonNumber = cartonViewModel.CartonNumber;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cartonViewModel);
        }

        // GET: Carton/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Carton carton = db.Cartons.Find(id);
            if (carton == null)
            {
                return HttpNotFound();
            }
            return View(carton);
        }

        // POST: Carton/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CartonBuilderSingletonUtil.Instance.DeleteAllCartondetail(id);
            Carton carton = db.Cartons.Find(id);
            db.Cartons.Remove(carton);
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

        public ActionResult AddEquipment(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var carton = db.Cartons
                .Where(c => c.Id == id)
                .Select(c => new CartonDetailsViewModel()
                {
                    CartonNumber = c.CartonNumber,
                    CartonId = c.Id
                })
                .SingleOrDefault();

            if (carton == null)
            {
                return HttpNotFound();
            }

            var equipment = db.Equipments
                .Where(e => !db.CartonDetails.Where(cd => cd.CartonId == id).Select(cd => cd.EquipmentId).Contains(e.Id))
                .Where(e => !(db.CartonDetails.Select(c => c.EquipmentId)).Contains(e.Id))
                .Select(e => new EquipmentViewModel()
                {
                    Id = e.Id,
                    ModelType = e.ModelType.TypeName,
                    SerialNumber = e.SerialNumber
                })
                .ToList();

            carton.Equipment = equipment;
            return View(carton);
        }

        /// <summary>
        /// This is to add the Equipment to the Carton; if the carton is having less than 10 items
        /// </summary>
        /// <param name="addEquipmentViewModel"></param>
        /// <returns></returns>
        public ActionResult AddEquipmentToCarton([Bind(Include = "CartonId,EquipmentId")] AddEquipmentViewModel addEquipmentViewModel)
        {
            if (addEquipmentViewModel != null)
            {
                int count = CartonBuilderSingletonUtil.Instance.getCartonCount(addEquipmentViewModel.CartonId);
                if (count < 10)
                {
                    var carton = db.Cartons
                        .Include(c => c.CartonDetails)
                        .Where(c => c.Id == addEquipmentViewModel.CartonId)
                        .SingleOrDefault();
                    if (carton == null)
                    {
                        return HttpNotFound();
                    }
                    var equipment = db.Equipments
                        .Where(e => e.Id == addEquipmentViewModel.EquipmentId)
                        .SingleOrDefault();
                    if (equipment == null)
                    {
                        return HttpNotFound();
                    }
                    var detail = new CartonDetail()
                    {
                        Carton = carton,
                        Equipment = equipment
                    };
                    carton.CartonDetails.Add(detail);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("AddEquipment", new { id = addEquipmentViewModel.CartonId });
        }
     
        /// <summary>
        /// This is to view the Carton and Equipment details for the given ID.
        /// If the ID is null it'll be a bad request
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ViewCartonEquipment(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var carton = db.Cartons
                .Where(c => c.Id == id)
                .Select(c => new CartonDetailsViewModel()
                {
                    CartonNumber = c.CartonNumber,
                    CartonId = c.Id,
                    Equipment = c.CartonDetails
                        .Select(cd => new EquipmentViewModel()
                        {
                            Id = cd.EquipmentId,
                            ModelType = cd.Equipment.ModelType.TypeName,
                            SerialNumber = cd.Equipment.SerialNumber
                        })
                })
                .SingleOrDefault();
            if (carton == null)
            {
                return HttpNotFound();
            }
            return View(carton);
        }

        /// <summary>
        /// This is to remove the Equipment from the Carton by the Carton Id and Equipment ID
        /// </summary>
        /// <param name="removeEquipmentViewModel"></param>
        /// <returns></returns>
        public ActionResult RemoveEquipmentOnCarton([Bind(Include = "CartonId,EquipmentId")] RemoveEquipmentViewModel removeEquipmentViewModel)
        {
            //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (ModelState.IsValid)
            {
                CartonDetail cartonDetail = db.CartonDetails.SingleOrDefault(c => c.EquipmentId == removeEquipmentViewModel.EquipmentId);
                if(cartonDetail != null)
                {
                    db.CartonDetails.Remove(cartonDetail);
                    db.SaveChanges();                    
                }
            }
            return RedirectToAction("ViewCartonEquipment", new { id = removeEquipmentViewModel.CartonId });
        }

        /// <summary>
        /// Facility to delete all the Equipments from the Carton in one go.
        /// </summary>
        /// <param name="CartonID"></param>
        /// <returns></returns>
        public ActionResult RemoveAllEquipmentOnCarton(int CartonID)
        {
            if (CartonID <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CartonBuilderSingletonUtil.Instance.DeleteAllCartondetail(CartonID);
            return RedirectToAction("ViewCartonEquipment", new { id = CartonID });
        }
    }
}
