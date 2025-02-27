﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net;
using INF2course.Attributes;
using INF2course.DAO;
using System.IO;

namespace INF2course.Controllers
{
    [HttpController("accounts")]
    public class Accounts: BaseController
    {
        DAOAccount dAO=new DAOAccount();

        [HttpGET("getaccounts")]
        public List<AccountInfo> GetAccounts()
        {
            if (GetCurrentAuthInfo() == null)
            {
                return null;
            }
            return dAO.GetAll();
        }
        [HttpPOST("saveaccount")]
        public bool SaveAccount(string login, string password)
        {

            AccountInfo existingAccount = dAO.GetByColumnValue("login", login);
            if (existingAccount != null)
            {
                Response.Redirect("/reg.html");
            }

            AccountInfo accountInfo = new AccountInfo
            {
                Login = login,
                Password = MD5Hash.HashPassword(password)
            };
            dAO.Insert(accountInfo);
            Response.Redirect("/profile.html");
            return true;
        }

        [HttpGET("logout")]
        public void Logout()
        {
            var cookie = Request.Cookies.FirstOrDefault(x => x.Name == "SessionId");
            if (cookie != null)
            {
                Guid sessionId = System.Text.Json.JsonSerializer.Deserialize<Guid>(cookie.Value);
                Request.Cookies.Remove(cookie);
                Response.Cookies.Add(new Cookie() { Name = "SessionId", Expires = DateTime.Now.AddDays(-1), Path= "/" });
                SessionManager.Delete(sessionId);
            }
           
            Response.Redirect("/");
        }

        [HttpPOST("login")]
        public bool Login(string login, string password)
        {
            AccountInfo existingAccount = dAO.GetByColumnValue("login", login);
            //var dictionary=
            if (existingAccount != null && existingAccount.Password == password)
            {
                Session session = SessionManager.Create(existingAccount.Id, existingAccount.Login);
                string cookie = System.Text.Json.JsonSerializer.Serialize(session.Id);
                Response.Cookies.Add(new Cookie("SessionId", cookie, path: "/"));
                Response.Redirect("/profile.html"); 
                return true;
            }
            Response.Redirect("/auth.html");
            return false;
        }

        private AuthInfo GetCurrentAuthInfo()
        {
            var cookie = Request.Cookies.FirstOrDefault(x => x.Name == "SessionId");
            if (cookie == null)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized; // 401
                return null;
            }
            Guid sessionId = System.Text.Json.JsonSerializer.Deserialize<Guid>(cookie.Value);            
            if (!SessionManager.IsValid(sessionId))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized; // 401
                return null;
            }

            Session session = SessionManager.GetById(sessionId);
            return new AuthInfo {  IsAuthorized = true, UserId = session.AccountId };
        }

        [HttpGET("getaccountinfo")]
        public string GetAccountInfo()
        {
            AuthInfo currentAuth = GetCurrentAuthInfo();
            if (currentAuth == null)
            {
                return null;
            }

            AccountInfo existingAccount = dAO.GetByColumnValue("id", currentAuth.UserId);
            //Razor
            string str = "@Model.answer";
            string template = RazorGenerator<object>.Run("get_users", str, 
                new { answer = $"Id: {existingAccount.Id}, Login: {existingAccount.Login}, Password: {existingAccount.Password}"} );

            return template;
        }

        [HttpGET("saveage")]
        public void SaveAge(int age)
        {
            var cookie = Request.Cookies.FirstOrDefault(x => x.Name == "SessionId");
            if (cookie != null)
            {
                Guid sessionId = System.Text.Json.JsonSerializer.Deserialize<Guid>(cookie.Value);
                var session = SessionManager.GetById(sessionId);
                if (session == null)
                {
                    return;
                }
                dAO.SaveAge(session.AccountId, age);
            }
        }

        [HttpGET("savecity")]
        public void SaveAdress(string city)
        {
            var cookie = Request.Cookies.FirstOrDefault(x => x.Name == "SessionId");
            if (cookie != null)
            {
                Guid sessionId = System.Text.Json.JsonSerializer.Deserialize<Guid>(cookie.Value);
                var session = SessionManager.GetById(sessionId);
                if (session == null)
                {
                    return;
                }
                dAO.SaveAdress(session.AccountId, city);
            }
        }

        [HttpGET("savename")]
        public void SaveName(string name)
        {
            var cookie = Request.Cookies.FirstOrDefault(x => x.Name == "SessionId");
            if (cookie != null)
            {
                Guid sessionId = System.Text.Json.JsonSerializer.Deserialize<Guid>(cookie.Value);
                var session = SessionManager.GetById(sessionId);
                if (session == null)
                {
                    return;
                }
                dAO.SaveName(session.AccountId, name);
            }
        }
        [HttpGET("savenumphone")]
        public void SaveNumPhone(string numphone)
        {
            var cookie = Request.Cookies.FirstOrDefault(x => x.Name == "SessionId");
            if (cookie != null)
            {
                Guid sessionId = System.Text.Json.JsonSerializer.Deserialize<Guid>(cookie.Value);
                var session = SessionManager.GetById(sessionId);
                if (session == null)
                {
                    return;
                }
                dAO.SaveNumPhone(session.AccountId, numphone);
            }
        }
    }
}
