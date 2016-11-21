using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GroupApp.Factory;
using Microsoft.AspNetCore.Mvc;
using group.Models;
using CryptoHelper;

namespace groups.Controllers
{
    public class GroupController : Controller
    {
        private readonly UserRepository userFactory;
         private readonly GroupRepository groupsFactory;

        public GroupController()
        {
            userFactory = new UserRepository();
            groupsFactory = new GroupRepository();
        }
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            if(TempData["errors"] != null)
            {
               ViewBag.errors = TempData["errors"];
            }
            return View("Login");
        }
// Post Methods:: Login, Registration
        [HttpPost]
        [Route("registration")]
        public IActionResult Create(User newuser)
        {   
            List<string> temp_errors = new List<string>();
            if(ModelState.IsValid)
            {
                 if(userFactory.FindEmail(newuser.email) == null){ // Checking email is registered previously
                    userFactory.Add(newuser);
                    ViewBag.User_Extracting = userFactory.FindByID();
                    int current_other_id = ViewBag.User_Extracting.id;
                    HttpContext.Session.SetInt32("current_id", (int) current_other_id);
                    return RedirectToAction("Dashboard");
                }
                 else{
                    temp_errors.Add("Email is already in use");
                    TempData["errors"] = temp_errors;
                    return RedirectToAction("Index");
                }
            }
            foreach(var error in ModelState.Values)
            {
                if(error.Errors.Count > 0)
                {
                    temp_errors.Add(error.Errors[0].ErrorMessage);
                }  
            }
            TempData["errors"] = temp_errors;
            return RedirectToAction("Index");
        }
        [HttpPost]
        [RouteAttribute("login")]
        public IActionResult Login(string email, string password)
        {
            List<string> temp_errors = new List<string>();
            if(email == null || password == null)
            {
                temp_errors.Add("Enter Email and Password Fields to Login");
                TempData["errors"] = temp_errors;
                return RedirectToAction("Index");
            }
//Login User Id Extracting query
          User check_user = userFactory.FindEmail(email);
            if(check_user == null)
            {
                temp_errors.Add("Email is not registered");
                TempData["errors"] = temp_errors;
                return RedirectToAction("Login");
            }
            bool correct = Crypto.VerifyHashedPassword((string) check_user.password, password);
            if(correct)
            {
                HttpContext.Session.SetInt32("current_id", check_user.id);
                return RedirectToAction("Dashboard");
            }
            else{
                temp_errors.Add("Password is not matching");
                TempData["errors"] = temp_errors;
                return RedirectToAction("Login");
            }
        }
 //Dashboard start
        [HttpGet]
        [Route("dashboard")]
        public IActionResult Dashboard()
        {
            //on refresh once after logout
            if(HttpContext.Session.GetInt32("current_id") == null)
            {
                return RedirectToAction("Index");
            }
             if(TempData["errors"] != null)
            {
               ViewBag.errors = TempData["errors"];
            }
            //Dashboard begins
            ViewBag.User_one = userFactory.CurrentUser((int)HttpContext.Session.GetInt32("current_id"));
            ViewBag.All_Groups = groupsFactory.AllGroups();
            return View("Dashboard");
        }
        [HttpPost]
        [Route("addgroup")]
        public IActionResult AddGroup(Group newgroup)
        {
            List<string> temp_errors = new List<string>();
            if(ModelState.IsValid)
            {
                 groupsFactory.AddGroup(newgroup);
                 Console.WriteLine("Group is Successfully added");
                 //Now add to joiners table
                 ViewBag.Group_Extracting = groupsFactory.Group_Last_ID(); // Extracting newly added group to extract its id
                 groupsFactory.Add_Joiner(ViewBag.Group_Extracting.id, (int)HttpContext.Session.GetInt32("current_id"));
                 return RedirectToAction("Dashboard");
            }
            foreach(var error in ModelState.Values)
            {
                if(error.Errors.Count > 0)
                {
                    temp_errors.Add(error.Errors[0].ErrorMessage);
                }  
            }
            TempData["errors"] = temp_errors;
            return RedirectToAction("Dashboard");
        }
//Deleting Group
        [HttpGet]
        [Route("deletegroup/{id}")]
        public IActionResult Group_Delete(string id = "")
        {
            groupsFactory.DeleteGroup(id);
            return RedirectToAction("dashboard");
        }
//Show individual groups
        [HttpGet]
        [Route("show/{id}")]
        public IActionResult Show(string id = "")
        {
            if(HttpContext.Session.GetInt32("current_id") == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.Group_Info = groupsFactory.Show_Info(id);
            ViewBag.Other_Users = groupsFactory.others(id);
            ViewBag.Switch = groupsFactory.Switch(id, (int)HttpContext.Session.GetInt32("current_id"));
            ViewBag.User_one = userFactory.CurrentUser((int)HttpContext.Session.GetInt32("current_id"));
            return View("ShowGroups");
        }
// Join Group
        [HttpGet]
        [Route("join/{id}")]
        public IActionResult Group_Join(string id = "")
        {
            Console.WriteLine("id is:::" + id);
            ViewBag.User_one = userFactory.CurrentUser((int)HttpContext.Session.GetInt32("current_id"));
            groupsFactory.Join_Group(id,(int)HttpContext.Session.GetInt32("current_id"));
            return RedirectToAction("Dashboard");
        }
//Leave Group
        [HttpGet]
        [Route("leave/{id}")]
        public IActionResult Group_Leave(string id = "")
        {
            ViewBag.User_one = userFactory.CurrentUser((int)HttpContext.Session.GetInt32("current_id"));
            groupsFactory.Leave_Group(id, (int)HttpContext.Session.GetInt32("current_id"));
            return RedirectToAction("Dashboard");
        }
// Logout
        [HttpGet]
        [Route("logout")]
         public IActionResult Logout()
         {
             HttpContext.Session.Clear();
             return RedirectToAction("Index");
         }
    }
}