using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace CustomsExternal.Controllers
{
    public class DeclarationController : ApiController
    {
        private CustomsExternalEntities db = new CustomsExternalEntities();
        private readonly HttpClient _httpClient;
        
        public DeclarationController()
        {
            _httpClient = new HttpClient();
        }
      
        [HttpGet]
        public Declarations Get(int decId)
        {
            return db.Declarations.FirstOrDefault(x => x.Id == decId);
        }

        [Route("api/Dec/GetAndSend/{id}")]
        [HttpPost]
        [ResponseType(typeof(Declarations))]
        public async Task<string> GetAndSendDeclarationAsync(int id, Declarations newdeclarations)
        {
            bool isSign = bool.Parse(HttpContext.Current.Request.QueryString["isSign"] ?? "false");

            var json = JsonConvert.SerializeObject(newdeclarations, Formatting.Indented);
            PutDeclarationsChangeAsync(id, newdeclarations);
            var decId = id;
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            string CustomsApiUrl = ConfigurationManager.AppSettings["CustomsApiUrl"];
            var response = await _httpClient.PostAsync($"{CustomsApiUrl}/Declaration/{decId}?isSign={isSign}&isExternal=true", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
            }
            else
            {
                throw new Exception("Error while posting data: " + response.StatusCode);
            }
        }

        // POST api/<DeclarationController>
        [HttpPost]
        public IHttpActionResult Post(Declarations declaration)
        {
            db.Declarations.Add(declaration);
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
            }

            return Ok(declaration);
        }        

        // DELETE api/<DeclarationController>/5
        [HttpDelete()]
        public void Delete(int id)
        {
        }

        [HttpPut()]
        [ResponseType(typeof(Declarations))]
        public IHttpActionResult PutDeclarationsChangeAsync(int id,Declarations newDeclaration)
        {
            var declarations = db.Declarations.Find(id);
            if (declarations == null)
            {
                return NotFound();
            }

            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}
            // -- declaration --
            declarations.CustomsStatus = newDeclaration.CustomsStatus;
            declarations.AgentFileReferenceID = newDeclaration.AgentFileReferenceID;
            declarations.AcceptanceDateTime = newDeclaration.AcceptanceDateTime;
            if (newDeclaration.DeclarationOfficeID != "")
                declarations.DeclarationOfficeID = newDeclaration.DeclarationOfficeID;
            declarations.DeclarationNumber = newDeclaration.DeclarationNumber;
            declarations.IssueDateTime = newDeclaration.IssueDateTime;
            declarations.TypeCode = newDeclaration.TypeCode;
            declarations.TaxationDateTime = newDeclaration.TaxationDateTime;
            declarations.ExternalDeclarationID = newDeclaration.ExternalDeclarationID;
            declarations.VersionID = newDeclaration.VersionID;
            declarations.ExpenseLoadingFactor = newDeclaration.ExpenseLoadingFactor;
            declarations.AutonomyRegionType = newDeclaration.AutonomyRegionType;
            declarations.TehilaDeclarationID = newDeclaration.TehilaDeclarationID;
            declarations.ReleaseDateTime = newDeclaration.ReleaseDateTime;
            declarations.TotalDealValueAmountNIS = newDeclaration.TotalDealValueAmountNIS;
            declarations.CifValueNIS = newDeclaration.CifValueNIS;
            declarations.TaxAssessedAmount = newDeclaration.TaxAssessedAmount;
            declarations.AgentID = newDeclaration.AgentID;
            declarations.RoleCode = newDeclaration.RoleCode;
            if (newDeclaration.GovernmentProcedure != "")
                declarations.GovernmentProcedure = newDeclaration.GovernmentProcedure;
            declarations.ImporterID = newDeclaration.ImporterID;
            declarations.ImporterRoleCode = newDeclaration.ImporterRoleCode;
            declarations.EntitlementTypeCode = newDeclaration.EntitlementTypeCode;
            declarations.IssueLocation = newDeclaration.IssueLocation;
            declarations.ImporterName = newDeclaration.ImporterName;
            declarations.ImporterCountry = newDeclaration.ImporterCountry;
            declarations.PreviousDocument = newDeclaration.PreviousDocument;
            declarations.PreviousDocumentType = newDeclaration.PreviousDocumentType;
            declarations.CreateDateTime = newDeclaration.CreateDateTime;
            declarations.DeclarationStatusCode = newDeclaration.DeclarationStatusCode;
            declarations.TotalMADDealValueAmountNIS = newDeclaration.TotalMADDealValueAmountNIS;

            // -- consignment --
            //if (newDeclaration.Consignments.ElementAt(0).ExportationCountryCode == null || newDeclaration.Consignments.ElementAt(0).ExportationCountryCode == "")
            //{

            //}
            Console.WriteLine(declarations.Consignments);
            declarations.Consignments.ElementAt(0).ExportationCountryCode = newDeclaration.Consignments.ElementAt(0).ExportationCountryCode;
            declarations.Consignments.ElementAt(0).LoadingLocation = newDeclaration.Consignments.ElementAt(0).LoadingLocation;
            declarations.Consignments.ElementAt(0).UnloadingLocationID = newDeclaration.Consignments.ElementAt(0).UnloadingLocationID;
            declarations.Consignments.ElementAt(0).TransportContractDocumentTypeCode = newDeclaration.Consignments.ElementAt(0).TransportContractDocumentTypeCode;
            declarations.Consignments.ElementAt(0).ArrivalDateTime = newDeclaration.Consignments.ElementAt(0).ArrivalDateTime;
            declarations.Consignments.ElementAt(0).TransportContractDocumentID = newDeclaration.Consignments.ElementAt(0).TransportContractDocumentID;
            declarations.Consignments.ElementAt(0).SecondCargoID = newDeclaration.Consignments.ElementAt(0).SecondCargoID;
            declarations.Consignments.ElementAt(0).ThirdCargoID = newDeclaration.Consignments.ElementAt(0).ThirdCargoID;
            declarations.Consignments.ElementAt(0).CargoDescription = newDeclaration.Consignments.ElementAt(0).CargoDescription;

            // -- ConsignmentPackagesMeasures --
            
            declarations.ConsignmentPackagesMeasures.ElementAt(0).PackageMeasureQualifier = newDeclaration.ConsignmentPackagesMeasures.ElementAt(0).PackageMeasureQualifier;
            declarations.ConsignmentPackagesMeasures.ElementAt(0).TypeCode = newDeclaration.ConsignmentPackagesMeasures.ElementAt(0).TypeCode;
            declarations.ConsignmentPackagesMeasures.ElementAt(0).TotalPackageQuantity = newDeclaration.ConsignmentPackagesMeasures.ElementAt(0).TotalPackageQuantity;
            declarations.ConsignmentPackagesMeasures.ElementAt(0).GrossMassMeasure = newDeclaration.ConsignmentPackagesMeasures.ElementAt(0).GrossMassMeasure;
            declarations.ConsignmentPackagesMeasures.ElementAt(0).MarksNumbers = newDeclaration.ConsignmentPackagesMeasures.ElementAt(0).MarksNumbers;

            //// -- ConsignmentRegisteredFacilities --

            var existingFacilities = db.ConsignmentRegisteredFacilities
        .Where(f => f.DeclarationId == id)
        .ToList();

            //Add new facility (ID=0)
            foreach (var newFacility in newDeclaration.ConsignmentRegisteredFacilities)
            {
                if (newFacility.FacilityID != "" && newFacility.Id == 0)
                {
                    var facilityToAdd = new ConsignmentRegisteredFacilities
                    {
                        FacilityID = newFacility.FacilityID,
                        FacilityType = newFacility.FacilityType,
                        FacilitySequenceNumeric = newFacility.FacilitySequenceNumeric,
                        ConsignmentId = declarations.Consignments.ElementAt(0).Id,
                        DeclarationId = declarations.Id,
                        //DeclarationId = id 
                    };

                    db.ConsignmentRegisteredFacilities.Add(facilityToAdd);
                }
            }

            //Updating existing facilities and removing old facilities that do not exist
            foreach (var existingFacility in existingFacilities)
            {
                var matchingNewFacility = newDeclaration.ConsignmentRegisteredFacilities
                    .FirstOrDefault(nf => nf.Id == existingFacility.Id);

                if (matchingNewFacility != null)
                {
                    //update
                    existingFacility.FacilityID = matchingNewFacility.FacilityID;
                    existingFacility.FacilityType = matchingNewFacility.FacilityType;
                    existingFacility.FacilitySequenceNumeric = matchingNewFacility.FacilitySequenceNumeric;
                }
                else
                {
                    db.ConsignmentRegisteredFacilities.Remove(existingFacility);
                }
            }

            // -- SupplierInvoices --

            var existingSupplierInvoices = declarations.SupplierInvoices;
            int i = 0;
            foreach (var newElement in newDeclaration.SupplierInvoices)
            {
                //if (newElement.Id < 0)
                //{
                //    var elementToRemove = existingSupplierInvoices.FirstOrDefault(x => x.Id == -(newElement.Id));
                //    if (elementToRemove != null)
                //    {
                //        db.Entry(elementToRemove).State = EntityState.Deleted;
                //        i++;
                //        continue;
                //    }

                //}
                //else if (newElement.Id > 0)
                //{
                //var existingElement = existingSupplierInvoices.FirstOrDefault(x => x.Id == newElement.Id);
                var existingElement = existingSupplierInvoices.ElementAt(0);
                if (existingElement != null)
                {
                    existingElement.InvoiceTypeCode = newElement.InvoiceTypeCode;
                        existingElement.LocationID = newElement.LocationID;
                        existingElement.SupplierID = newElement.SupplierID;
                        existingElement.TradeTermsConditionCode = newElement.TradeTermsConditionCode;
                        existingElement.CurrencyCode = newElement.CurrencyCode;
                        existingElement.InvoiceNumber = newElement.InvoiceNumber;
                        existingElement.IssueDateTime = newElement.IssueDateTime;
                        existingElement.InvoiceAmount = newElement.InvoiceAmount;
                }
                //}
                //else
                //{
                //    var newSupplierInvoice = new SupplierInvoices
                //    {
                //        InvoiceTypeCode = newElement.InvoiceTypeCode,
                //        LocationID = newElement.LocationID,
                //        SupplierID = newElement.SupplierID,
                //        TradeTermsConditionCode = newElement.TradeTermsConditionCode,
                //        CurrencyCode = newElement.CurrencyCode,
                //        InvoiceNumber = newElement.InvoiceNumber,
                //        IssueDateTime = newElement.IssueDateTime,
                //        InvoiceAmount = newElement.InvoiceAmount,
                //    };

                //    existingSupplierInvoices.Add(newSupplierInvoice);
                //}
                //}

                //for (int i = 0; i < newDeclaration.SupplierInvoices.Count; i++)
                //{
                //    declarations.SupplierInvoices.ElementAt(i).InvoiceTypeCode = newDeclaration.SupplierInvoices.ElementAt(i).InvoiceTypeCode;
                //    declarations.SupplierInvoices.ElementAt(i).LocationID = newDeclaration.SupplierInvoices.ElementAt(i).LocationID;
                //    declarations.SupplierInvoices.ElementAt(i).SupplierID = newDeclaration.SupplierInvoices.ElementAt(i).SupplierID;
                //    declarations.SupplierInvoices.ElementAt(i).TradeTermsConditionCode = newDeclaration.SupplierInvoices.ElementAt(i).TradeTermsConditionCode;
                //    declarations.SupplierInvoices.ElementAt(i).CurrencyCode = newDeclaration.SupplierInvoices.ElementAt(i).CurrencyCode;
                //    declarations.SupplierInvoices.ElementAt(i).InvoiceNumber = newDeclaration.SupplierInvoices.ElementAt(i).InvoiceNumber;
                //    declarations.SupplierInvoices.ElementAt(i).IssueDateTime = newDeclaration.SupplierInvoices.ElementAt(i).IssueDateTime;
                //    declarations.SupplierInvoices.ElementAt(i).InvoiceAmount = newDeclaration.SupplierInvoices.ElementAt(i).InvoiceAmount;

                // --  update or insert of SupplierInvoiceItems CustomsValuation --
                //var existingValuations = declarations.SupplierInvoices.ElementAt(i).CustomsValuation;


                //foreach (var newCustomsValuation in newElement.CustomsValuation)
                //{
                //    if (newCustomsValuation.ID < 0)
                //    {
                //        var elementToRemove = existingValuations.FirstOrDefault(x => x.ID == -(newCustomsValuation.ID));
                //        if (elementToRemove != null)
                //        {
                //            //existingValuations.Remove(elementToRemove); 
                //            db.Entry(elementToRemove).State = EntityState.Deleted;

                //        }
                //    }
                //    else if (newCustomsValuation.ID > 0)
                //    {
                //        var existingElement = existingValuations.FirstOrDefault(x => x.ID == newCustomsValuation.ID);
                //        if (existingElement != null)
                //        {
                //            existingElement.ChargesTypeCode = newCustomsValuation.ChargesTypeCode;
                //            existingElement.ExitToEntryChargeAmount = newCustomsValuation.ExitToEntryChargeAmount;
                //            existingElement.FreightChargeAmount = newCustomsValuation.FreightChargeAmount;
                //            existingElement.OtherChargeDeductionAmount = newCustomsValuation.OtherChargeDeductionAmount;
                //            existingElement.PaymentTermsCode = newCustomsValuation.PaymentTermsCode;
                //            existingElement.CurrencyCode = newCustomsValuation.CurrencyCode;
                //        }
                //    }
                //    else
                //    {
                //        var newValuation = new CustomsValuation
                //        {
                //            ChargesTypeCode = newCustomsValuation.ChargesTypeCode,
                //            ExitToEntryChargeAmount = newCustomsValuation.ExitToEntryChargeAmount,
                //            FreightChargeAmount = newCustomsValuation.FreightChargeAmount,
                //            OtherChargeDeductionAmount = newCustomsValuation.OtherChargeDeductionAmount,
                //            PaymentTermsCode = newCustomsValuation.PaymentTermsCode,
                //            CurrencyCode = newCustomsValuation.CurrencyCode
                //        };

                //        existingValuations.Add(newValuation);
                //    }
                //}



                // update or insert of SupplierInvoiceItems
                var existingItems = declarations.SupplierInvoices.ElementAt(i).SupplierInvoiceItems;
                if (newElement.SupplierInvoiceItems.Count() > 1)
                {

                }

                for (int j = 0; j < newElement.SupplierInvoiceItems.Count; j++)
                {
                    var newItem = newElement.SupplierInvoiceItems.ElementAt(j);

                    if (newItem.Id < 0)
                    {
                        var elementToRemove = existingItems.FirstOrDefault(x => x.Id == -(newItem.Id));
                        if (elementToRemove != null)
                        {
                            db.Entry(elementToRemove).State = EntityState.Deleted;
                        }
                    }

                    else if (newItem.Id > 0)
                    {
                        var existingItem = existingItems.FirstOrDefault(x => x.Id == newItem.Id);
                        if (existingItem != null)
                        {
                            // update item
                            existingItem.CustomsBookType = newItem.CustomsBookType;
                            existingItem.TaxExemptCode = newItem.TaxExemptCode;
                            existingItem.OptionalTama = newItem.OptionalTama;
                            existingItem.SalesTaxExemptionType = newItem.SalesTaxExemptionType;
                            existingItem.IsUsed = newItem.IsUsed;
                            existingItem.DutyRegimeCode = newItem.DutyRegimeCode;
                            existingItem.OriginCountryCode = newItem.OriginCountryCode;
                            existingItem.AmountType = newItem.AmountType;
                            existingItem.CustomsValueAmount = newItem.CustomsValueAmount;
                            existingItem.ClassificationID = newItem.ClassificationID;
                            existingItem.ClassificationIdentificationTypeCode = newItem.ClassificationIdentificationTypeCode;
                            existingItem.TariffQuantity = newItem.TariffQuantity;
                            existingItem.MeasureQualifier = newItem.MeasureQualifier;
                        }
                    }
                    else
                    {
                        //add new item
                        var newItemToAdd = new SupplierInvoiceItems
                        {
                            CustomsBookType = newItem.CustomsBookType,
                            TaxExemptCode = newItem.TaxExemptCode,
                            OptionalTama = newItem.OptionalTama,
                            SalesTaxExemptionType = newItem.SalesTaxExemptionType,
                            IsUsed = newItem.IsUsed,
                            DutyRegimeCode = newItem.DutyRegimeCode,
                            OriginCountryCode = newItem.OriginCountryCode,
                            AmountType = newItem.AmountType,
                            CustomsValueAmount = newItem.CustomsValueAmount,
                            ClassificationID = newItem.ClassificationID,
                            ClassificationIdentificationTypeCode = newItem.ClassificationIdentificationTypeCode,
                            TariffQuantity = newItem.TariffQuantity
                        };

                        existingItems.Add(newItemToAdd);
                    }
                }
                i++;
            }


            // -- DeclarationTaxes --
            if (newDeclaration.DeclarationTaxes.Count > 0)
            {
                // declarations.DeclarationTaxes.Clear();
                var taxesToRemove = db.DeclarationTaxes
            .Where(t => t.DeclarationId == id)
            .ToList();

                // הסר את כל המיסים שנמצאו
                db.DeclarationTaxes.RemoveRange(taxesToRemove);
                foreach (var newTax in newDeclaration.DeclarationTaxes)
                {
                    var taxToUpdate = new DeclarationTaxes
                    {
                        AdValoremTaxBaseAmount = newTax.AdValoremTaxBaseAmount,
                        TaxTypeCode = newTax.TaxTypeCode,
                        Amount = newTax.Amount,
                        DeferedTaxAmount = newTax.DeferedTaxAmount
                    };

                    // Add the new tax to the declaration
                    declarations.DeclarationTaxes.Add(taxToUpdate);
                }
            }
            try
            {
                db.SaveChanges();
            }

            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return Ok(declarations);

        }
    }
}
