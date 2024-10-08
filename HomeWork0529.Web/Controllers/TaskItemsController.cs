﻿using HomeWork0529.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;

namespace HomeWork0529.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskItemsController : ControllerBase
    {
        private readonly string _connectionString;

        private readonly IHubContext<TaskItemHub> _hub;

        public TaskItemsController(IConfiguration configuration, IHubContext<TaskItemHub> hub)
        {
            _connectionString = configuration.GetConnectionString("ConStr");
            _hub = hub;
        }

        [HttpPost]
        [Route("getall")]
        public void GetAll(TaskItem taskItem)
        {
            var repo = new TaskRepository(_connectionString);
            repo.GetAll();
            _hub.Clients.All.SendAsync("newTaskReceived", taskItem);
        }

        [HttpPost]
        [Route("add")]
        public void Add(TaskItem taskItem)
        {
            var user = GetCurrentUser();
            var repo = new TaskRepository(_connectionString);
            repo.Add(taskItem);
        }

        [HttpPost]
        [Authorize]
        [Route("updatestatus")]
        public void UpdateStatus(int id)
        {
            var user = GetCurrentUser();
            var repo = new TaskRepository(_connectionString);
            repo.UpdateStatus($"{user.FirstName} {user.LastName} is doing this", id, user.Id);
            _hub.Clients.All.SendAsync("statusUpdate", $"{user.FirstName} {user.LastName} is doing this", user.Id, id);
        }

        [HttpPost]
        [Route("completetask")]
        public void CompleteTask(int id)
        {
            var repo = new TaskRepository(_connectionString);
            repo.DeleteTask(id);
            _hub.Clients.All.SendAsync("completedtask", id);
        }

        private User GetCurrentUser()
        {
            var userRepo = new UserRepository(_connectionString);
            var user = userRepo.GetByEmail(User.Identity.Name);
            return user;
        }
    }
}
