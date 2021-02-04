using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CandidateProject.Utils
{
    public sealed class CartonBuilderSingletonUtil
    {
        private CartonContext db = new CartonContext();
        private static readonly Lazy<CartonBuilderSingletonUtil>
           lazy =
           new Lazy<CartonBuilderSingletonUtil>
               (() => new CartonBuilderSingletonUtil());

        public static CartonBuilderSingletonUtil Instance { get { return lazy.Value; } }

        private CartonBuilderSingletonUtil()
        {
        }

        /// <summary>
        /// Singleton function to delete all the Carton details
        /// All the Equipments are deleted from the carton on a single click
        /// </summary>
        /// <param name="id"></param>
        public void DeleteAllCartondetail(int id)
        {
            var items = db.CartonDetails.Where(carton => carton.CartonId == id);
            if (items.Any())
                db.CartonDetails.RemoveRange(items);
            db.SaveChanges();
        } 

        /// <summary>
        /// Function that returns the carton count for the given Carton ID
        /// </summary>
        /// <param name="cartonId"></param>
        /// <returns></returns>
        internal int getCartonCount(int cartonId)
        {
            return db.CartonDetails.Where(c => c.CartonId == cartonId).Count();
        }
    }
}