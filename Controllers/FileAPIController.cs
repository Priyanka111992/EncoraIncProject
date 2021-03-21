using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

using System.IO;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
namespace Save_Files_Database_EF_MVC.Controllers
{
    public class FileAPIController : ApiController
    {
        [HttpPost]
        [Route("api/FileAPI/SaveFile")]
        public HttpResponseMessage SaveFile()
        {
            //Create HTTP Response.
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

            //Check if Request contains File.
            if (HttpContext.Current.Request.Files.Count == 0)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            //Read the File data from Request.Form collection.
            HttpPostedFile postedFile = HttpContext.Current.Request.Files[0];

            //Convert the File data to Byte Array.
            byte[] bytes;
            using (BinaryReader br = new BinaryReader(postedFile.InputStream))
            {
                bytes = br.ReadBytes(postedFile.ContentLength);
            }

            //Insert the File to Database Table.
            FilesEntities entities = new FilesEntities();
            tblFile file = new tblFile
            {
                Name = Path.GetFileName(postedFile.FileName),
                ContentType = postedFile.ContentType,
                Data = bytes
            };
            entities.tblFiles.Add(file);
            entities.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK, new { id = file.id, Name = file.Name });
        }

        [HttpPost]
        [Route("api/FileAPI/GetFiles")]
        public HttpResponseMessage GetFiles()
        {
            FilesEntities entities = new FilesEntities();
            var files = from file in entities.tblFiles
                        select new { id = file.id, Name = file.Name };
            return Request.CreateResponse(HttpStatusCode.OK, files);
        }

        [HttpGet]
        [Route("api/FileAPI/GetFile")]
        public HttpResponseMessage GetFile(int fileId)
        {
            //Create HTTP Response.
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

            //Fetch the File data from Database.
            FilesEntities entities = new FilesEntities();
            tblFile file = entities.tblFiles.ToList().Find(p => p.id == fileId);

            //Set the Response Content.
            response.Content = new ByteArrayContent(file.Data);

            //Set the Response Content Length.
            response.Content.Headers.ContentLength = file.Data.LongLength;

            //Set the Content Disposition Header Value and FileName.
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = file.Name;

            //Set the File Content Type.
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            return response;
        }
    }
}
