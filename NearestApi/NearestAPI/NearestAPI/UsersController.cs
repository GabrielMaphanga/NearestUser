using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NearestAPI
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private static List<User> Users = new List<User>();

        private static readonly List<Hotel> Hotels = new List<Hotel>
        {
            new Hotel { Name = "A", Lat = -43.9509, Lon = -34.4618 },
            new Hotel { Name = "B", Lat = 40.7128, Lon = -74.0060 },
            new Hotel { Name = "C", Lat = 34.0522, Lon = -118.2437 },
            new Hotel { Name = "D", Lat = -25.2744, Lon = 133.7751 }
        };

        [HttpGet]
        [Route("api/users", Name = "Nearest Userss")]
        public IActionResult GetAllNearest()
        {
            var result = Users.Select(u =>
            {
                var nearestHotel = Hotels
                    .Select(h => new
                    {
                        Hotel = h,
                        Distance = HaversineDistance(u.Lat, u.Lon, h.Lat, h.Lon)
                    })
                    .OrderBy(h => h.Distance)
                    .First();

                return new
                {
                    u.Id,
                    u.Name,
                    u.City,
                    u.Lat,
                    u.Lon,
                    NearestHotel = new { nearestHotel.Hotel.Name, Distance = nearestHotel.Distance }
                };
            });

            return Ok(result);
        }

        [HttpGet("Hotels")]
       
        public IActionResult GetAll()
        {
           

            return Ok(Hotels);
        }


        [HttpPost]
        public IActionResult Add(User user)
        {
            user.Id = Users.Count > 0 ? Users.Max(u => u.Id) + 1 : 1;
            Users.Add(user);
            return Ok(user);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, User updated)
        {
            var user = Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();

            user.Name = updated.Name;
            user.City = updated.City;
            user.Lat = updated.Lat;
            user.Lon = updated.Lon;

            return Ok(user);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var user = Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();

            Users.Remove(user);
            return NoContent();
        }

        private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371; // km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRadians(double deg) => deg * Math.PI / 180;
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    public class Hotel
    {
        public string Name { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
    }
}

