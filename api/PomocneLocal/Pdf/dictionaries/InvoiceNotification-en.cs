namespace bp.ot.s.API.PDF.Translations {
    public class InvoiceNotificationEnglish : IInvoiceNotificationTranslation
    {
        public string CityAndDate => "Mściszewo, dnia: ";

        public string Title => "Final demand of payment";

        public string ActingOnBehalf =>"Acting of behalf ";

        public string WithItsBusiness => " with its business sets in ";
        
        public string HerebyRequest => "I hereby request a payment of the amount of ";

        public string ForTheTransportOrder => " for the transport order, Invoice: ";

        public string PleaseFindEnclosed => " Please find enclosed a dated copy of the relevant invoice for your reference.";

        public string PaymentByABankDetail => "The payment by a bank transfer should be made to the bank details as follows: ";

        public string WithinDays => " within 3 (three) days of the date of receiving this letter.";

        public string IfThePaymentIsNotReceived => "If the payment is not received within the aforementioned period I reserve the right to take legal action to recover the monies without further notice to you, which will charge you with additional high court fees and late payment interest.";

        public string IfPaymentIsAlreadyPaid => "If the invoice has already been paid, please send a confirmation of the transfer for verification in our books. E- mail adress: payments@offertrans.pl";

        public string YoursSincerely =>"Yours sincerely";

        public string Attachment => "Attachments:";

        public string AttachmentInfo => "Invoice No: ";
    }
}