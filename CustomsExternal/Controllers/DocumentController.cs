using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace CustomsExternal.Controllers
{
    public class DocumentController : ApiController
    {
        private CustomsExternalEntities db = new CustomsExternalEntities();

        // GET: api/Documents
        [Route("api/Document/{id}")]
        public IHttpActionResult GetDocumentById(decimal id)
        {
            var document = db.Documents.Find(id);

            if (document == null)
            {
                return NotFound();
            }

            return Ok(document);
        }


        // GET: api/Documents/5
        [ResponseType(typeof(Documents))]
        [Route("api/Document/entity/{entityId}")]

        public IHttpActionResult GetDocuments(decimal entityId)
        {
            Declarations dec = db.Declarations.FirstOrDefault(x => x.Id == entityId);
            if (dec == null)
            {
                return NotFound();
            }
            var documents = db.Documents.Where(x => x.RelatedID == dec.Id).ToList();

            if (!documents.Any())
            {
                return NotFound();
            }

            return Ok(documents);
        }

        // PUT: api/Documents/5
        [ResponseType(typeof(void))]
        [HttpPut]
        [Route("api/Document/{id}")]
        public IHttpActionResult PutDocuments(int id, Documents documents)
        {
            Declarations dec = db.Declarations.FirstOrDefault(x => x.Id == documents.RelatedID);

            //Declarations dec = db.Declarations.FirstOrDefault(x => x.Id == documents.RelatedID);
            if (dec == null)
            {
                //    dec = db.Declarations.FirstOrDefault(x => x.Id == documents.RelatedID);
                //    if(dec == null)
                //    {
                return BadRequest("dec not found");
                //}
            }
            //documents.RelatedID = dec.Id;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != documents.Id)
            {
                return BadRequest();
            }

            db.Entry(documents).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(documents); ;
        }

        // POST: api/Documents
        [ResponseType(typeof(Documents))]
        public IHttpActionResult PostDocuments(Documents documents)
        {
            Declarations dec = db.Declarations.FirstOrDefault(x => x.Id == documents.RelatedID);
            if (dec == null)
            {
                // return NotFound(); 
            }
            //documents.RelatedID = dec.Id;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Documents.Add(documents);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = documents.Id }, documents);
        }

        // DELETE: api/Documents/5
        [HttpDelete]
        [Route("api/Document" + "/{id}")]
        [ResponseType(typeof(Documents))]
        public IHttpActionResult DeleteDocuments(int id)
        {
            Documents documents = db.Documents.Find(id);
            if (documents == null)
            {
                return NotFound();
            }

            db.Documents.Remove(documents);
            db.SaveChanges();

            return Ok(new { message = "Document deleted successfully" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool DocumentsExists(int id)
        {
            return db.Documents.Count(e => e.Id == id) > 0;
        }

        [HttpGet]
        [Route("api/docByDecId")]
        public IHttpActionResult GetDocsByDecId(int declarationId)
        {

            var results = db.Documents
                .Where(m => m.RelatedID == declarationId)
                .ToList();

            return Ok(results);
        }
    }
}
