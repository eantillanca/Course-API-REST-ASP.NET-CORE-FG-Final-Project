using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MoviesAPI;
using MoviesAPI.Helpers;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace MoviesAPITest;

public class BaseTest
{
    protected ApplicationDbContext BuildContext(string dbName = "TestDatabase")
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    protected IMapper ConfigAutoMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            cfg.AddProfile(new AutoMapperProfile(geometryFactory));
        });

        return config.CreateMapper();
    }
}
