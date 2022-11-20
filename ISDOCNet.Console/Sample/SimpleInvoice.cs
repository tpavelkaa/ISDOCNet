using DocuWare.Platform.ServerClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;

namespace ISDOCNet.Console.Sample
{
    public class SimpleInvoiceVat
    {
        private static DocuWare.Platform.ServerClient.Document _doc;

        public static Invoice Create(DocuWare.Platform.ServerClient.Document doc)
        {
            _doc = doc;
            System.Console.WriteLine("Document ID: " + doc.Id);
            System.Console.WriteLine("Document Name: " + doc.Fields.FirstOrDefault(x => x.FieldLabel == "IČ dodavatele").Item);
            var customerName = ParseField("Odběratel");
            var customerICO = ParseField("IČ odběratele");
            var customerContact = new Contact(customerName, "", "");
            var customerAddress = new PostalAddress(Country.CzechRepulic(), "", "", "", "");
            var supplierName = ParseField("Dodavatel");
            var supplierICO = ParseField("IČ dodavatele");
            var supplierDIC = ParseField("DIČ dodavatele");
            var supplierRegisterInformation = "";
            var supplierContact = new Contact(supplierName, "", "");
            var supplierAddress = new PostalAddress(Country.CzechRepulic(), "", "", "", "");

            var result = new ISDOCNet.Invoice();
            result.version = "6.0.1";
            result.LocalCurrencyCode = "CZK";
            result.CurrRate = 1;
            result.ID = ParseField("Evidenční číslo dokladu"); //Invoice Number
            result.UUID = Guid.NewGuid().ToString(); // Unique inovice identification
            result.IssueDate = Convert.ToDateTime(ParseField("Datum vystavení")).Date; //Invoice Date
            if (DateTime.TryParse(ParseField("DUZP"), out DateTime res) == true)
            {
                result.TaxPointDate = res.Date; //Tax Point Date
            }

            result.VATApplicable = true; //With VAT
            result.RefCurrRate = 1; // CZK To CZK
            result.ElectronicPossibilityAgreementReference = new Note();
            result.BuyerCustomerParty = new BuyerCustomerParty();
            result.BuyerCustomerParty.Party = new Party();
            result.BuyerCustomerParty.Party.Contact = customerContact;
            result.BuyerCustomerParty.Party.PartyIdentification = new PartyIdentification("Odběratel", "", customerICO);
            result.BuyerCustomerParty.Party.PartyName = new PartyName(customerName);
            result.BuyerCustomerParty.Party.PostalAddress = customerAddress;
            result.AccountingCustomerParty = new AccountingCustomerParty();
            result.AccountingCustomerParty.Party = new Party();
            result.AccountingCustomerParty.Party.Contact = customerContact;
            result.AccountingCustomerParty.Party.PartyIdentification = new PartyIdentification("Odběratel", "", customerICO);
            result.AccountingCustomerParty.Party.PartyName = new PartyName(customerName);
            result.AccountingCustomerParty.Party.PostalAddress = customerAddress;
            result.SellerSupplierParty = new SellerSupplierParty();
            result.SellerSupplierParty.Party = new Party();
            result.SellerSupplierParty.Party.Contact = supplierContact;
            result.SellerSupplierParty.Party.PartyIdentification = new PartyIdentification("Dodavatel", "", supplierICO);
            result.SellerSupplierParty.Party.PartyName = new PartyName(supplierName);
            result.SellerSupplierParty.Party.PostalAddress = supplierAddress;
            result.SellerSupplierParty.Party.PartyTaxScheme = new PartyTaxScheme();
            result.SellerSupplierParty.Party.PartyTaxScheme.CompanyID = supplierDIC;
            result.SellerSupplierParty.Party.PartyTaxScheme.TaxScheme = "VAT";
            result.SellerSupplierParty.Party.RegisterIdentification = new RegisterIdentification(supplierRegisterInformation);
            result.AccountingSupplierParty = new AccountingSupplierParty();
            result.AccountingSupplierParty.Party = new Party();
            result.AccountingSupplierParty.Party.Contact = supplierContact;
            result.AccountingSupplierParty.Party.PartyIdentification = new PartyIdentification("Dodavatel", "", supplierICO);
            result.AccountingSupplierParty.Party.PartyName = new PartyName(supplierName);
            result.AccountingSupplierParty.Party.PostalAddress = supplierAddress;
            result.AccountingSupplierParty.Party.PartyTaxScheme = new PartyTaxScheme();
            result.AccountingSupplierParty.Party.PartyTaxScheme.CompanyID = supplierDIC;
            result.AccountingSupplierParty.Party.PartyTaxScheme.TaxScheme = "VAT";
            result.AccountingSupplierParty.Party.RegisterIdentification = new RegisterIdentification(supplierRegisterInformation);
            var i = 1;

            //foreach (var item in invoice.InvoiceItems)
            //{
            //    var line = new InvoiceLine();
            //    line.ClassifiedTaxCategory = new ClassifiedTaxCategory();
            //    line.ClassifiedTaxCategory.Percent = item.TaxVat.Value;
            //    line.ClassifiedTaxCategory.VATApplicable = item.TaxVat.Value >= 0;
            //    line.ClassifiedTaxCategory.VATCalculationMethod = VATCalculationMethod.Item0;
            //    line.ID = i.ToString();
            //    line.InvoicedQuantity = new Quantity(item.UnitType.Code, item.Quantity);
            //    line.Item = new Item();
            //    line.Item.Description = item.Name;
            //    line.UnitPrice = item.Price;
            //    line.UnitPriceTaxInclusive = item.PriceVAT;
            //    line.LineExtensionAmount = item.Price;
            //    line.LineExtensionAmountTaxInclusive = item.PriceVAT;
            //    line.LineExtensionTaxAmount = item.PriceVAT - item.Price;

            //    if (result.InvoiceLines == null)
            //        result.InvoiceLines = new List<InvoiceLine>();
            //    result.InvoiceLines.Add(line);
            //    i++;
            //}

            var line = new InvoiceLine();
            line.ClassifiedTaxCategory = new ClassifiedTaxCategory();
            line.ClassifiedTaxCategory.Percent = 21; //VAT
            line.ClassifiedTaxCategory.VATApplicable = true;
            line.ClassifiedTaxCategory.VATCalculationMethod = VATCalculationMethod.Item0;
            line.ID = i.ToString();
            line.InvoicedQuantity = new Quantity("KS", 1);
            line.Item = new Item();
            line.Item.Description = "Product";
            line.UnitPrice = 100;
            line.UnitPriceTaxInclusive = 121;
            line.LineExtensionAmount = 100;
            line.LineExtensionAmountTaxInclusive = 121;
            line.LineExtensionTaxAmount = 21;

            if (result.InvoiceLines == null)
                result.InvoiceLines = new List<InvoiceLine>();
            result.InvoiceLines.Add(line);
            i++;

            result.TaxTotal = new TaxTotal();
            result.TaxTotal.TaxAmount = 0;
            result.TaxTotal.TaxSubTotal = new List<TaxSubTotal>();
            foreach (var taxGroup in result.InvoiceLines.GroupBy(p => p.ClassifiedTaxCategory.Percent))
            {
                var taxSubTotal = new TaxSubTotal();
                taxSubTotal.TaxableAmount = taxGroup.Sum(p => p.LineExtensionAmount);
                taxSubTotal.TaxInclusiveAmount = taxGroup.Sum(p => p.LineExtensionAmountTaxInclusive);
                taxSubTotal.TaxAmount = taxSubTotal.TaxInclusiveAmount - taxSubTotal.TaxableAmount;
                if (taxGroup.Key.Value > 0)
                {
                    taxSubTotal.TaxCategory = new TaxCategory();
                    taxSubTotal.TaxCategory.VATApplicable = true;
                    taxSubTotal.TaxCategory.Percent = taxGroup.Key.Value;
                }
                else
                {
                    taxSubTotal.TaxCategory = new TaxCategory();
                    taxSubTotal.TaxCategory.VATApplicable = false;
                    taxSubTotal.TaxCategory.Percent = 0;
                }

                taxSubTotal.AlreadyClaimedTaxableAmount = 0;
                taxSubTotal.AlreadyClaimedTaxAmount = 0;
                taxSubTotal.AlreadyClaimedTaxInclusiveAmount = 0;
                taxSubTotal.DifferenceTaxableAmount = taxSubTotal.TaxableAmount - taxSubTotal.AlreadyClaimedTaxableAmount;
                taxSubTotal.DifferenceTaxAmount = taxSubTotal.TaxAmount - taxSubTotal.AlreadyClaimedTaxAmount;
                taxSubTotal.DifferenceTaxInclusiveAmount = taxSubTotal.TaxInclusiveAmount - taxSubTotal.AlreadyClaimedTaxInclusiveAmount;
                result.TaxTotal.TaxSubTotal.Add(taxSubTotal);
            }

            result.LegalMonetaryTotal = new LegalMonetaryTotal();
            result.LegalMonetaryTotal.TaxExclusiveAmount = result.InvoiceLines.Sum(p => p.LineExtensionAmount);
            result.LegalMonetaryTotal.TaxInclusiveAmount = result.InvoiceLines.Sum(p => p.LineExtensionAmountTaxInclusive);
            result.LegalMonetaryTotal.AlreadyClaimedTaxExclusiveAmount = 0;
            result.LegalMonetaryTotal.AlreadyClaimedTaxInclusiveAmount = 0;
            result.LegalMonetaryTotal.DifferenceTaxExclusiveAmount = result.LegalMonetaryTotal.TaxExclusiveAmount - result.LegalMonetaryTotal.AlreadyClaimedTaxExclusiveAmount;
            result.LegalMonetaryTotal.DifferenceTaxInclusiveAmount = result.LegalMonetaryTotal.TaxInclusiveAmount - result.LegalMonetaryTotal.AlreadyClaimedTaxInclusiveAmount;
            result.LegalMonetaryTotal.PayableRoundingAmount = 0;
            result.LegalMonetaryTotal.PaidDepositsAmount = 0;
            result.LegalMonetaryTotal.PayableAmount = result.LegalMonetaryTotal.TaxInclusiveAmount - result.LegalMonetaryTotal.PaidDepositsAmount;
            return result;
        }

        private string GetStringFromTable(DocumentIndexFieldTableRow row, string fieldLabel)
        {
            string? returnedRow = row.ColumnValue.FirstOrDefault(x => x.FieldLabel == fieldLabel)?.Item?.ToString() ?? string.Empty;

            return returnedRow;
        }

        private float GetFloatFromTable(DocumentIndexFieldTableRow row, string fieldLabel)
        {
            string? valueToString = row.ColumnValue.FirstOrDefault(x => x.FieldLabel == fieldLabel)?.Item?.ToString();

            float returnedRow = 0.0F;

            if (valueToString != null)
            {
                returnedRow = float.Parse(valueToString);
            }

            return returnedRow;
        }

        private static string ParseField(string fieldLabel)
        {
            string? item = null;
            if (_doc is null)
            {
                throw new Exception("Document document used in ParseField method is null.");
            }

            DocumentIndexField? field = _doc.Fields.FirstOrDefault(x => x.FieldLabel == fieldLabel);

            if (field != null && field.Item != null)
            {
                item = field.Item.ToString();
            }
            else
            {
                item = string.Empty;
            }

#pragma warning disable CS8603 // Possible null reference return.
            return item;
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
