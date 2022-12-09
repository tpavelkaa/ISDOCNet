using DocuWare.Platform.ServerClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Xml.Linq;

namespace ISDOCNet.Console.Sample
{
    public class SimpleInvoiceVat
    {
        private static DocuWare.Platform.ServerClient.Document _doc;

        public static Invoice Create(DocuWare.Platform.ServerClient.Document doc)
        {
            _doc = doc;

            if (_doc.FileDownloadRelationLink is null)
            {
                _doc = _doc.GetDocumentFromSelfRelation();
            }
            //System.Console.WriteLine("Document ID: " + doc.Id);
            //System.Console.WriteLine("Document Name: " + doc.Fields.FirstOrDefault(x => x.FieldLabel == "IČ dodavatele").Item);

            ///
            /// VARIABLES
            ///

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
            var bankAccount = ParseField("Číslo bankovního účtu");
            var bankCode = ParseField("Kód banky");
            var tryDateReceived = DateTime.TryParse(ParseField("Datum doručení"), out var dateReceived);
            var tryDateDue = DateTime.TryParse(ParseField("Datum splatnosti"), out var dateDue);
            var tryDateIssued = DateTime.TryParse(ParseField("Datum vystavení"), out var dateIssued);
            var tryDateTax = DateTime.TryParse(ParseField("DUZP"), out var dateTax);
            var tryBaseAmount10 = decimal.TryParse(ParseField("Základ 10 %"), out var baseAmount10);
            var tryvatAmount10 = decimal.TryParse(ParseField("DPH 10 %"), out var vatAmount10);
            var trybaseAmount15 = decimal.TryParse(ParseField("Základ 15 %"), out var baseAmount15);
            var tryvatAmount15 = decimal.TryParse(ParseField("DPH 15 %"), out var vatAmount15);
            var trybaseAmount21 = decimal.TryParse(ParseField("Základ 21 %"), out var baseAmount21);
            var tryvatAmount21 = decimal.TryParse(ParseField("DPH 21 %"), out var vatAmount21);
            var invoiceNumber = ParseField("Evidenční číslo dokladu"); //Invoice Number
            var tryDateFrom = DateTime.TryParse(ParseField("Období od"), out var dateFrom);
            var trydateTo = DateTime.TryParse(ParseField("Období do"), out var dateTo);
            var tryToBePaid = decimal.TryParse(ParseField("Částka k úhradě"), out var toBePaid);
            var tryBaseTotal = decimal.TryParse(ParseField("Základy celkem"), out var baseTotal);
            var tryTaxTotal = decimal.TryParse(ParseField("DPH celkem"), out var taxTotal);
            var variableSymbol = ParseField("Variabilní symbol");
            var invoiceDescription = ParseField("Předmět faktury");
            var docType = ParseField("Typ dokumentu");
            var invoiceType = ParseField("Typ faktury");
            var version = "6.0.2";
            var guid = Guid.NewGuid().ToString();
            var paymentType = ParseField("ISDOC platba");
            var bank = ParseField("Banka");
            var iban = ParseField("IBAN");
            var constSymbol = ParseField("Konstantní symbol");
            var bic = ParseField("BIC");

            // Switch for paymenType and PaymentMeansCode enum
            PaymentMeansCode paymentMeansCode;
            switch (paymentType)
            {
                case "Platba v hotovosti":
                    paymentMeansCode = PaymentMeansCode.Item10;
                    break;
                case "Platba šekem":
                    paymentMeansCode = PaymentMeansCode.Item20;
                    break;
                case "Uskutečněná kreditní transakce":
                    paymentMeansCode = PaymentMeansCode.Item31;
                    break;
                case "Převod na účet":
                    paymentMeansCode = PaymentMeansCode.Item42;
                    break;
                case "Platba kartou":
                    paymentMeansCode = PaymentMeansCode.Item48;
                    break;
                case "Inkaso":
                    paymentMeansCode = PaymentMeansCode.Item49;
                    break;
                case "Platba dobírkou":
                    paymentMeansCode = PaymentMeansCode.Item50;
                    break;
                case "Zaúčtování mezi partnery":
                    paymentMeansCode = PaymentMeansCode.Item97;
                    break;
                case "":
                    paymentMeansCode = PaymentMeansCode.Item42;
                    break;
                default: 
                    paymentMeansCode = PaymentMeansCode.Item42;
                    break;
            }

            ///
            /// ISDOC INVOICE
            ///

            var result = new ISDOCNet.Invoice();
            result.version = version;
            result.DocumentType = DocumentType.Item1;
            result.ID = invoiceNumber; //Invoice Number
            result.UUID = guid; // Unique inovice identification
            result.IssuingSystem = "DocuWare"; // Issuing system
            
            if (tryDateIssued) result.IssueDate = dateIssued; // Issue date
            if (tryDateTax) result.TaxPointDate = dateTax; // Tax point date

            if (supplierDIC is not null) result.VATApplicable = true; // VAT applicable
            else result.VATApplicable = false;
            
            result.ElectronicPossibilityAgreementReference = new Note();
            result.LocalCurrencyCode = "CZK";
            result.CurrRate = 1;
            result.RefCurrRate = 1; // CZK To CZK

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

            result.InvoiceLines = new List<InvoiceLine>();
            if (ParseInvoiceItems("Vyúčtování spotřeby") is not null)
            {
                result.InvoiceLines = ParseInvoiceItems("Vyúčtování spotřeby");
            }

            //var i = 1;

            //foreach (var item in result.InvoiceLines)
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

            //var line = new InvoiceLine();
            //line.ClassifiedTaxCategory = new ClassifiedTaxCategory();
            //line.ClassifiedTaxCategory.Percent = 21; //VAT
            //line.ClassifiedTaxCategory.VATApplicable = true;
            //line.ClassifiedTaxCategory.VATCalculationMethod = VATCalculationMethod.Item0;
            //line.ID = i.ToString();
            //line.InvoicedQuantity = new Quantity("KS", 1);
            //line.Item = new Item();
            //line.Item.Description = "Product";
            //line.UnitPrice = 100;
            //line.UnitPriceTaxInclusive = 121;
            //line.LineExtensionAmount = 100;
            //line.LineExtensionAmountTaxInclusive = 121;
            //line.LineExtensionTaxAmount = 21;

            //if (result.InvoiceLines == null)
            //    result.InvoiceLines = new List<InvoiceLine>();
            //result.InvoiceLines.Add(line);
            //i++;
            result.OrderReferences = new List<OrderReference>();
            result.DeliveryNoteReferences = new List<DeliveryNoteReference>();

            result.TaxTotal = new TaxTotal();
            result.TaxTotal.TaxAmount = taxTotal;
            result.TaxTotal.TaxSubTotal = new List<TaxSubTotal>();
            if (result.InvoiceLines is not null && result.InvoiceLines.Count > 0)
            {

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
                result.LegalMonetaryTotal.PayableAmount = toBePaid;
                result.LegalMonetaryTotal.PaidDepositsAmount = 0;
            }
            else
            {
                result.InvoiceLines.Add(
                    new InvoiceLine()
                    {
                        ID = "1",
                        InvoicedQuantity = new Quantity("KS", 1),
                        Item = new Item()
                        {
                            Description = invoiceDescription
                        },
                        UnitPrice = baseTotal,
                        UnitPriceTaxInclusive = toBePaid,
                        LineExtensionAmount = baseTotal,
                        LineExtensionAmountTaxInclusive = toBePaid,
                        LineExtensionTaxAmount = taxTotal,
                        ClassifiedTaxCategory = new ClassifiedTaxCategory()
                        {
                            Percent = 21,
                            VATApplicable = true,
                            VATCalculationMethod = VATCalculationMethod.Item0
                        }
                    });


                if (trybaseAmount21)
                {
                    result.TaxTotal.TaxSubTotal.Add(new TaxSubTotal()
                    {
                        TaxableAmount = baseAmount21,
                        TaxAmount = vatAmount21,
                        TaxInclusiveAmount = baseAmount21 + vatAmount21,
                        AlreadyClaimedTaxAmount = 0,
                        AlreadyClaimedTaxableAmount = 0,
                        AlreadyClaimedTaxInclusiveAmount = 0,
                        DifferenceTaxableAmount = baseAmount21,
                        DifferenceTaxAmount = vatAmount21,
                        DifferenceTaxInclusiveAmount = baseAmount21 + vatAmount21,

                        TaxCategory = new TaxCategory()
                        {
                            VATApplicable = true,
                            Percent = 21.00m                             
                        }
                    });
                }
                
                if (trybaseAmount15)
                {
                    result.TaxTotal.TaxSubTotal.Add(new TaxSubTotal()
                    {
                        TaxableAmount = baseAmount15,
                        TaxAmount = vatAmount15,
                        TaxInclusiveAmount = baseAmount15 + vatAmount15,
                        AlreadyClaimedTaxAmount = 0,
                        AlreadyClaimedTaxableAmount = 0,
                        AlreadyClaimedTaxInclusiveAmount = 0,
                        DifferenceTaxableAmount = baseAmount15,
                        DifferenceTaxAmount = vatAmount15,
                        DifferenceTaxInclusiveAmount = baseAmount15 + vatAmount15,

                        TaxCategory = new TaxCategory()
                        {
                            VATApplicable = true,
                            Percent = 15.00m
                        }
                    });
                }

                if (tryBaseAmount10)
                {
                    result.TaxTotal.TaxSubTotal.Add(new TaxSubTotal()
                    {
                        TaxableAmount = baseAmount10,
                        TaxAmount = vatAmount10,
                        TaxInclusiveAmount = baseAmount10 + vatAmount10,
                        AlreadyClaimedTaxAmount = 0,
                        AlreadyClaimedTaxableAmount = 0,
                        AlreadyClaimedTaxInclusiveAmount = 0,
                        DifferenceTaxableAmount = baseAmount10,
                        DifferenceTaxAmount = vatAmount10,
                        DifferenceTaxInclusiveAmount = baseAmount10 + vatAmount10,


                        TaxCategory = new TaxCategory()
                        {
                            VATApplicable = true,
                            Percent = 15.00m
                        }
                    });
                }

                result.LegalMonetaryTotal = new LegalMonetaryTotal();
                result.LegalMonetaryTotal.TaxExclusiveAmount = baseTotal;
                result.LegalMonetaryTotal.TaxInclusiveAmount = toBePaid;
                result.LegalMonetaryTotal.AlreadyClaimedTaxExclusiveAmount = 0;
                result.LegalMonetaryTotal.AlreadyClaimedTaxInclusiveAmount = 0;
                result.LegalMonetaryTotal.DifferenceTaxExclusiveAmount = result.LegalMonetaryTotal.TaxExclusiveAmount - result.LegalMonetaryTotal.AlreadyClaimedTaxExclusiveAmount;
                result.LegalMonetaryTotal.DifferenceTaxInclusiveAmount = result.LegalMonetaryTotal.TaxInclusiveAmount - result.LegalMonetaryTotal.AlreadyClaimedTaxInclusiveAmount;
                result.LegalMonetaryTotal.PayableRoundingAmount = 0;
                result.LegalMonetaryTotal.PaidDepositsAmount = 0;
                result.LegalMonetaryTotal.PayableAmount = toBePaid;

            }
            result.PaymentMeans = new PaymentMeans();
            result.PaymentMeans.Payment = new Payment();
            result.PaymentMeans.Payment.partialPayment = false;
            result.PaymentMeans.Payment.PaidAmount = toBePaid;
            result.PaymentMeans.Payment.PaymentMeansCode = paymentMeansCode;            

            result.PaymentMeans.Payment.Details = new Details();
            result.PaymentMeans.Payment.Details.PaymentDueDate = dateDue.ToString("yyyy-MM-dd");
            result.PaymentMeans.Payment.Details.ID = bankAccount; 
            result.PaymentMeans.Payment.Details.BankCode = bankCode; 
            result.PaymentMeans.Payment.Details.Name = bank;
            result.PaymentMeans.Payment.Details.IBAN = iban;
            result.PaymentMeans.Payment.Details.BIC = bic;
            result.PaymentMeans.Payment.Details.VariableSymbol = variableSymbol;
            result.PaymentMeans.Payment.Details.ConstantSymbol = constSymbol;

            return result;
        }

        private static string GetStringFromTable(DocumentIndexFieldTableRow row, string fieldLabel)
        {
            string? returnedRow = row.ColumnValue.FirstOrDefault(x => x.FieldLabel == fieldLabel)?.Item?.ToString() ?? string.Empty;

            return returnedRow;
        }

        private static decimal GetDecFromTable(DocumentIndexFieldTableRow row, string fieldLabel)
        {
            string? valueToString = row.ColumnValue.FirstOrDefault(x => x.FieldLabel == fieldLabel)?.Item?.ToString();

            decimal returnedRow = 0.0m;

            if (valueToString != null)
            {
                returnedRow = decimal.Parse(valueToString);
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

        private static List<InvoiceLine>? ParseInvoiceItems(string fieldName)
        {
            if (_doc is null)
            {
                throw new Exception("Document document used in ParseInvoiceItems method is null.");
            }

            List<InvoiceLine> invoiceItems = new();
            DocumentIndexField? field = _doc.Fields.FirstOrDefault(x => x.FieldLabel == fieldName);
            if (field is null || field.Item is null)
            {
                throw new Exception("Field " + fieldName + " is null");
            }

            if (field.Item is not DocumentIndexFieldTable)
            {
                throw new Exception("Not table field!");
            }

            DocumentIndexFieldTable tableFields = (DocumentIndexFieldTable)field.Item;
            foreach (DocumentIndexFieldTableRow row in tableFields.Row)
            {
                var line = new InvoiceLine();
                line.ClassifiedTaxCategory = new ClassifiedTaxCategory();
                line.ClassifiedTaxCategory.Percent = GetDecFromTable(row, "Sazba DPH");

                if (GetDecFromTable(row, "Sazba DPH") > 0)
                {
                    line.ClassifiedTaxCategory.VATApplicable = true;
                    line.ClassifiedTaxCategory.VATCalculationMethod = VATCalculationMethod.Item0;
                }
                else
                {
                    line.ClassifiedTaxCategory.VATApplicable = false;
                }
                line.ID = tableFields.Row.FindIndex(o => o.GetHashCode() == row.GetHashCode()).ToString();
                line.InvoicedQuantity = new Quantity(GetStringFromTable(row, "Měrná jednotka"), (GetDecFromTable(row, "Množství")));
                line.Item = new Item();
                line.Item.Description = GetStringFromTable(row, "Popis");
                line.UnitPrice = GetDecFromTable(row, "Jednotková cena");
                line.UnitPriceTaxInclusive = GetDecFromTable(row, "Celkem s DPH");
                line.LineExtensionAmount = GetDecFromTable(row, "Cena bez DPH");
                line.LineExtensionAmountTaxInclusive = GetDecFromTable(row, "Celkem s DPH");
                line.LineExtensionTaxAmount = GetDecFromTable(row, "DPH částka");

                invoiceItems.Add(line);
            }

            if (invoiceItems.Count > 0)
            {
                return invoiceItems;
            }
            else
            {
                return null;
            }
        }        
    }
}
