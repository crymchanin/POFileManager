#region Пространства имен
using POFileManager.ru.crimeanpost;
using System;
using System.Linq;
using System.Net;
#endregion


namespace POFileManager.Mail {
    public static class MailHelper {

        #region Члены и свойства класса
        /// <summary>
        /// Имя пользователя отправителя на почтовом сервере
        /// </summary>
        public static string Username { get; set; }

        /// <summary>
        /// Пароль пользователя отправителя на почтовом сервере
        /// </summary>
        public static string Password { get; set; }

        /// <summary>
        /// Домен связанный с учетными данными отправителя
        /// </summary>
        public static string Domain { get; set; }

        /// <summary>
        /// Адрес почтового сервера
        /// </summary>
        public static string Host { get; set; }

        /// <summary>
        /// E-mail получателя
        /// </summary>
        public static string ToRecipient { get; set; }
        #endregion


        /// <summary>
        /// Отправляет электронное письмо
        /// </summary>
        /// <param name="subject">Тема письма</param>
        /// <param name="body">Текст письма</param>
        public static void SendMail(string subject, string body) {
            using (ExchangeServiceBinding bind = new ExchangeServiceBinding()) {
                bind.Credentials = new NetworkCredential(Username, Password, Domain);
                bind.Url = "https://" + Host + "/EWS/Exchange.asmx";
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
                message.ToRecipients[0].EmailAddress = ToRecipient;

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
