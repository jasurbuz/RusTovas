﻿using BuxoroIlmZiyo.Services.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Yol.Data.Models;
using Yol.Services.IRepository;
using YolData;

namespace Yol.Services.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        #region Private mambers
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _host;
        
        private IGenericRepository<Company> _companies;
        private IGenericRepository<Road> _roads;
        private IGenericRepository<Coordinate> _coordinates;
        private IGenericRepository<CoordinateValue> _values;
        #endregion

        #region Constructors
        public UnitOfWork(AppDbContext context, IHostingEnvironment hostEnvironmnet)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            _host = hostEnvironmnet ?? throw new ArgumentNullException(nameof(hostEnvironmnet));
        }

        #endregion

        #region Public mambers
        public IGenericRepository<Company> Companies => _companies ??= new GenericRepository<Company>(_context);
        public IGenericRepository<Road> Roads => _roads ??= new GenericRepository<Road>(_context);
        public IGenericRepository<Coordinate> Coordinates => _coordinates ??= new GenericRepository<Coordinate>(_context);
        public IGenericRepository<CoordinateValue> Values => _values ??= new GenericRepository<CoordinateValue>(_context);

        public void Dispose()
        {
            _context.Dispose();

            GC.SuppressFinalize(this);
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
        #endregion

        #region File functions

        public async Task<string> SaveFileAsync(IFormFile file, string folder = "Images")
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = new String(Path.GetFileNameWithoutExtension(file.FileName).Take(10).ToArray()).Replace(' ', '-');
            fileName = fileName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(_host.ContentRootPath, folder, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return fileName;
        }

        public void DeleteFile(string fileName, string folder = "Images")
        {
            var filePath = Path.Combine(_host.ContentRootPath, folder, fileName);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        #endregion
    }
}
