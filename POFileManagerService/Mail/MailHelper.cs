#region Пространства имен
using POFileManagerService.ru.crimeanpost.cas;
using System;
using System.Linq;
using System.Net;
#endregion


namespace POFileManagerService.Mail {
    public static class MailHelper {

        #region Члены и свойства класса
        /// <summary>
        /// Параметры почты
        /// </summary>
        public static Configuration.Mail MailConfiguration { get; set; }
        #endregion


        /// <summary>
        /// Отправляет электронное письмо
        /// </summary>
        /// <param name="subject">Тема письма</param>
        /// <param name="body">Текст письма</param>
        public static void SendMail(string subject, string body) {
            using (ExchangeServiceBinding bind = new ExchangeServiceBinding()) {
                bind.Credentials = new NetworkCredential(MailConfiguration.Username, MailConfiguration.Password, MailConfiguration.Domain);
                bind.Url = "https://" + MailConfiguration.Host + "/EWS/Exchange.asmx";
                bind.RequestServerVersionValue = new RequestServerVersion();
                bind.RequestServerVersionValue.Version = ExchangeVersionType.Exchange2007_SP1;

                CreateItemType createItemRequest =
                    new CreateItemType {
                        Items = new NonEmptyArrayOfAllItemsType(),
                        MessageDispositionSpecified = true,
                        MessageDisposition = MessageDispositionType.SendOnly
                    };

                MessageType message = new MessageType();
                message.ToRecipients = new EmailAddressType[1];
                message.ToRecipients[0] = new EmailAddressType();
                message.ToRecipients[0].EmailAddress = MailConfiguration.ToRecipient;

                message.Subject = subject;

                message.Body = new BodyType();
                message.Body.BodyType1 = BodyTypeType.Text;
                message.Body.Value = body;

                createItemRequest.Items.Items = new ItemType[1];
                createItemRequest.Items.Items[0] = message;

                CreateItemResponseType createItemResponse = bind.CreateItem(createItemRequest);
                ArrayOfResponseMessagesType responseMessages = createItemResponse.ResponseMessages;

                ResponseMessageType[] responseMessage = responseMessages.Items;
                foreach (ResponseMessageType rmt in responseMessage.Where(rmt => rmt.ResponseClass == ResponseClassType.Error)) {
                    throw new Exception(rmt.MessageText);
                }
            }
        }
    }
}
