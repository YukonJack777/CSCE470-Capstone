using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Telerik.Web.UI;

using System.Text;


    public class ExchangeSchedulerProvider : SchedulerProviderBase
    {
        public const string ExchangeIdAttribute = "_ExchangeId";
        public const string ExchangeChangeKeyAttribute = "_ExchangeChangeKey";


        private ExchangeServiceBinding _service;
        private string _serverUrl;
        private string _username;
        private string _password;
        private string _domain;
        private NetworkCredential _credentials;


        protected ExchangeServiceBinding Service
        {
            get
            {
                if (_service == null)
                {
                    _service = GetExchangeBinding();
                }

                return _service;
            }

            set { _service = value; }
        }

        protected string ServerUrl
        {
            get { return _serverUrl; }
            set { _serverUrl = value; }
        }

        protected string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        protected string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        protected string Domain
        {
            get { return _domain; }
            set { _domain = value; }
        }

        protected NetworkCredential Credentials
        {
            get
            {
                if (_credentials == null)
                {
                    _credentials = new NetworkCredential(Username, Password, Domain);
                }

                return _credentials;
            }

            set { _credentials = value; }
        }
        public string CustomCalendarName
        {
            get;
            set;
        }

        public ExchangeSchedulerProvider(string serverUrl, string username, string password, string domain, string calendarName)
            : this()
        {
            ServerUrl = serverUrl;
            Username = username;
            Password = password;
            Domain = domain;
            CustomCalendarName = calendarName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeSchedulerProvider"/> class.
        /// </summary>
        /// <param name="serverUrl">The exchange web service end-point URL. For example "http://dc1.litwareinc.com/EWS/Exchange.asmx".</param>
        /// <param name="username">The username to use for authentication.</param>
        /// <param name="password">The password to use for authentication.</param>
        /// <param name="domain">The domain name to use for authentication.</param>
        public ExchangeSchedulerProvider(string serverUrl, string username, string password, string domain)
            : this()
        {
            ServerUrl = serverUrl;
            Username = username;
            Password = password;
            Domain = domain;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeSchedulerProvider"/> class.
        /// </summary>
        /// <param name="serverUrl">The exchange web service end-point URL. For example "http://dc1.litwareinc.com/EWS/Exchange.asmx".</param>
        /// <param name="credentials">Network credentials to use when connecting to the server.</param>
        public ExchangeSchedulerProvider(string serverUrl, NetworkCredential credentials)
            : this()
        {
            ServerUrl = serverUrl;
            Credentials = credentials;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeSchedulerProvider"/> class.
        /// </summary>
        public ExchangeSchedulerProvider()
        {

        }


        /// <summary>
        /// Reads the appointments from the Exchange server.
        /// </summary>
        /// <param name="owner">The owner RadScheduler instance.</param>
        /// <returns></returns>
        public override IEnumerable<Appointment> GetAppointments(RadScheduler owner)
        {
            List<ItemIdType> itemIds = new List<ItemIdType>();
            foreach (CalendarItemType item in FindCalendarItems())
            {
                if ((item.Start < owner.VisibleRangeEnd && item.End > owner.VisibleRangeStart) ||
                    item.CalendarItemType1 == CalendarItemTypeType.RecurringMaster)
                {
                    itemIds.Add(item.ItemId);
                }
            }

            return GetAppointments(owner, itemIds.ToArray());
        }

        /// <summary>
        /// Inserts the specified appointment.
        /// </summary>
        /// <param name="owner">The owner RadScheduler instance.</param>
        /// <param name="appointmentToInsert">The appointment to insert.</param>
        public override void Insert(RadScheduler owner, Appointment appointmentToInsert)
        {
            CreateRecurrenceExceptionContext createExceptionContext = owner.ProviderContext as CreateRecurrenceExceptionContext;
            if (createExceptionContext != null)
            {
                Debug.Assert(appointmentToInsert.RecurrenceState == RecurrenceState.Exception);
                InsertRecurrenceException(owner, appointmentToInsert, createExceptionContext.RecurrenceExceptionDate);
                return;
            }

            CalendarItemType calendarItem = CreateCalendarItem(appointmentToInsert, owner);

            CreateItemType createItemRequest = new CreateItemType();
            createItemRequest.SendMeetingInvitations = CalendarItemCreateOrDeleteOperationType.SendToNone;
            createItemRequest.SendMeetingInvitationsSpecified = true;
            createItemRequest.Items = new NonEmptyArrayOfAllItemsType();
            createItemRequest.Items.Items = new CalendarItemType[] { calendarItem };

            CreateItemResponseType response = Service.CreateItem(createItemRequest);
            ItemInfoResponseMessageType responseMessage = (ItemInfoResponseMessageType)response.ResponseMessages.Items[0];

            if (responseMessage.ResponseCode != ResponseCodeType.NoError)
            {
                throw new Exception("CreateItem failed with response code " + responseMessage.ResponseCode);
            }

            // DEMO: Creating attachments
            // CreateAttachment(responseMessage.Items.Items[0].ItemId);
        }

        /// <summary>
        /// Updates the specified appointment.
        /// </summary>
        /// <param name="owner">The owner RadScheduler instance.</param>
        /// <param name="appointmentToUpdate">The appointment to update.</param>
        public override void Update(RadScheduler owner, Appointment appointmentToUpdate)
        {
            if (owner.ProviderContext is RemoveRecurrenceExceptionsContext)
            {
                RemoveRecurrenceExceptions(appointmentToUpdate);
                return;
            }

            if (owner.ProviderContext is UpdateAppointmentContext)
            {
                // When removing recurrences through the UI,
                // one Update operation is used to both update the appointment
                // and remove the recurrence exceptions.
                RecurrenceRule updatedRule;
                RecurrenceRule.TryParse(appointmentToUpdate.RecurrenceRule, out updatedRule);

                if (updatedRule != null && updatedRule.Exceptions.Count == 0)
                {
                    RemoveRecurrenceExceptions(appointmentToUpdate);
                }
            }

            CreateRecurrenceExceptionContext createExceptionContext = owner.ProviderContext as CreateRecurrenceExceptionContext;
            if (createExceptionContext == null)
            {
                // We are not creating a recurrence exceptions - synchronize deleted occurrences.
                SynchronizeDeletedOccurrences(appointmentToUpdate);
            }

            ItemChangeType[] changes = new ItemChangeType[] { GetAppointmentChanges(appointmentToUpdate) };
            UpdateCalendarItem(changes);
        }

        /// <summary>
        /// Deletes the specified appointment.
        /// </summary>
        /// <param name="owner">The owner RadScheduler instance.</param>
        /// <param name="appointmentToDelete">The appointment to delete.</param>
        public override void Delete(RadScheduler owner, Appointment appointmentToDelete)
        {
            if (owner.ProviderContext is RemoveRecurrenceExceptionsContext)
            {
                return;
            }

            ItemIdType itemId = new ItemIdType();
            itemId.Id = appointmentToDelete.Attributes[ExchangeIdAttribute];
            itemId.ChangeKey = appointmentToDelete.Attributes[ExchangeChangeKeyAttribute];

            DeleteItem(itemId);
        }

        /// <summary>
        /// Implement this method if you use custom resources.
        /// </summary>
        public override IEnumerable<ResourceType> GetResourceTypes(RadScheduler owner)
        {
            return new ResourceType[] { };
        }

        /// <summary>
        /// Implement this method if you use custom resources.
        /// </summary>
        public override IEnumerable<Resource> GetResourcesByType(RadScheduler owner, string resourceType)
        {
            return new Resource[] { };
        }

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">The name of the provider is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"></see> on a provider after the provider has already been initialized.</exception>
        /// <exception cref="T:System.ArgumentException">The name of the provider has a length of zero.</exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (string.IsNullOrEmpty(name))
            {
                name = "ExchangeSchedulerProvider";
            }

            base.Initialize(name, config);

            ServerUrl = config["serverUrl"];
            if (string.IsNullOrEmpty(ServerUrl))
            {
                throw new ProviderException("Missing Exchange server URL. Please specify it with the serverUrl property. For example serverUrl=\"https://dc1.litwareinc.com\".");
            }

            Username = config["username"];
            if (string.IsNullOrEmpty(Username))
            {
                throw new ProviderException("Missing username. Please specify it with the username property. For example username=\"wl\".");
            }

            Password = config["password"];
            if (string.IsNullOrEmpty(Password))
            {
                throw new ProviderException("Missing password. Please specify it with the password property. For example password=\"pass@word1\".");
            }

            Domain = config["domain"];
            if (string.IsNullOrEmpty(Domain))
            {
                throw new ProviderException("Missing domain name. Please specify it with the domain property. For example domain=\"litwareinc\".");
            }
        }

        /// <exclude />
        /// <excludetoc />
        /// <summary>
        ///		For testing only
        /// </summary>
        public IEnumerable<Appointment> GetAllAppointments(RadScheduler owner)
        {
            List<ItemIdType> itemIds = new List<ItemIdType>();
            foreach (CalendarItemType item in FindCalendarItems())
            {
                itemIds.Add(item.ItemId);
            }

            return GetAppointments(owner, itemIds.ToArray());
        }

        protected virtual CalendarItemType CreateCalendarItem(Appointment apt, RadScheduler owner)
        {
            CalendarItemType calendarItem = new CalendarItemType();
            calendarItem.Subject = apt.Subject;
            calendarItem.Start = apt.Start;
            calendarItem.StartSpecified = true;
            calendarItem.End = apt.End;
            calendarItem.EndSpecified = true;

            calendarItem.MeetingTimeZone = GetTimeZone(owner.TimeZoneOffset);

            RecurrenceRule rrule;
            RecurrenceRule.TryParse(apt.RecurrenceRule, out rrule);
            if (rrule != null && rrule.Pattern.Frequency != RecurrenceFrequency.Hourly)
            {
                calendarItem.Recurrence = CreateRecurrence(rrule, owner);
            }

            return calendarItem;
        }

        protected virtual TimeZoneType GetTimeZone(TimeSpan tzOffset)
        {
            TimeZoneType tz = new TimeZoneType();
            string sign = tzOffset.TotalHours < 0 ? "-" : "";
            tz.BaseOffset = String.Format("{0}PT{1}H", sign, (int)Math.Abs(tzOffset.TotalHours));
            return tz;
        }

        protected virtual ItemChangeType GetAppointmentChanges(Appointment apt)
        {
            ItemChangeType itemUpdates = new ItemChangeType();

            ItemIdType itemId = new ItemIdType();
            itemId.Id = apt.Attributes[ExchangeIdAttribute];
            itemId.ChangeKey = apt.Attributes[ExchangeChangeKeyAttribute];

            itemUpdates.Item = itemId;
            List<ItemChangeDescriptionType> updates = new List<ItemChangeDescriptionType>();
            updates.Add(GetSubjectUpdate(apt));
            updates.Add(GetStartUpdate(apt));
            updates.Add(GetEndUpdate(apt));

            if (apt.RecurrenceRule != string.Empty)
            {
                updates.Add(GetRecurrenceUpdate(apt));
            }

            itemUpdates.Updates = updates.ToArray();

            return itemUpdates;
        }

        protected internal CalendarItemType[] FindCalendarItems()
        {
            // Form the FindItem request.
            FindItemType findItemRequest = new FindItemType();

            // Define the item properties that are returned in the response.
            ItemResponseShapeType itemProperties = new ItemResponseShapeType();
            itemProperties.BaseShape = DefaultShapeNamesType.IdOnly;

            PathToUnindexedFieldType calendarIsRecurringFieldPath = new PathToUnindexedFieldType();
            calendarIsRecurringFieldPath.FieldURI = UnindexedFieldURIType.calendarIsRecurring;

            PathToUnindexedFieldType calendarItemTypeFieldPath = new PathToUnindexedFieldType();
            calendarIsRecurringFieldPath.FieldURI = UnindexedFieldURIType.calendarCalendarItemType;

            PathToUnindexedFieldType calendarStartFieldPath = new PathToUnindexedFieldType();
            calendarStartFieldPath.FieldURI = UnindexedFieldURIType.calendarStart;

            PathToUnindexedFieldType calendarEndFieldPath = new PathToUnindexedFieldType();
            calendarEndFieldPath.FieldURI = UnindexedFieldURIType.calendarEnd;

            itemProperties.AdditionalProperties = new PathToUnindexedFieldType[]
			                                      	{
			                                      		calendarIsRecurringFieldPath, 
			                                      		calendarItemTypeFieldPath,
			                                      		calendarStartFieldPath,
			                                      		calendarEndFieldPath
			                                      	};
            findItemRequest.ItemShape = itemProperties;

            // Identify which folders to search.
            List<BaseFolderIdType> folderIDs = FindCalendarFolderByName(CustomCalendarName);
            //List<DistinguishedFolderIdType> folderIDs = new List<DistinguishedFolderIdType>();

            //DistinguishedFolderIdType ownCalendar = new DistinguishedFolderIdType();
            //ownCalendar.Id = DistinguishedFolderIdNameType.calendar;
            //folderIDs.Add(ownCalendar);

            //DistinguishedFolderIdType otherCalendar = new DistinguishedFolderIdType();
            //otherCalendar.Id = DistinguishedFolderIdNameType.calendar;

            //EmailAddressType otherMailbox = new EmailAddressType();
            //otherMailbox.EmailAddress = "xxxx@test.com";

            //otherCalendar.Mailbox = otherMailbox;

            //folderIDs.Add(otherCalendar);

            findItemRequest.ParentFolderIds = folderIDs.ToArray();

            // Define the sort order of items.
            FieldOrderType[] fieldsOrder = new FieldOrderType[1];
            fieldsOrder[0] = new FieldOrderType();
            PathToUnindexedFieldType subjectOrder = new PathToUnindexedFieldType();
            subjectOrder.FieldURI = UnindexedFieldURIType.calendarStart;
            fieldsOrder[0].Item = subjectOrder;
            fieldsOrder[0].Order = SortDirectionType.Ascending;
            findItemRequest.SortOrder = fieldsOrder;

            // Define the traversal type.
            findItemRequest.Traversal = ItemQueryTraversalType.Shallow;

            // Send the FindItem request and get the response.
            FindItemResponseType findItemResponse = Service.FindItem(findItemRequest);

            // Access the response message.
            ArrayOfResponseMessagesType responseMessages = findItemResponse.ResponseMessages;

            List<CalendarItemType> calendarItems = new List<CalendarItemType>();
            foreach (ResponseMessageType responseMessage in responseMessages.Items)
            {
                if (responseMessage is FindItemResponseMessageType)
                {
                    FindItemResponseMessageType firmt = (responseMessage as FindItemResponseMessageType);
                    FindItemParentType fipt = firmt.RootFolder;
                    object obj = fipt.Item;

                    if (obj is ArrayOfRealItemsType)
                    {
                        ArrayOfRealItemsType items = (obj as ArrayOfRealItemsType);

                        if (items.Items != null)
                        {
                            foreach (ItemType item in items.Items)
                            {
                                CalendarItemType calendarItem = item as CalendarItemType;

                                if (calendarItem != null)
                                {
                                    calendarItems.Add(calendarItem);
                                }
                            }
                        }
                    }
                }
            }

            return calendarItems.ToArray();
        }

        protected internal IList<CalendarItemType> GetCalendarItems(ItemIdType[] itemIds)
        {
            if (itemIds.Length == 0)
            {
                return new CalendarItemType[0];
            }

            List<CalendarItemType> calendarItems = new List<CalendarItemType>(itemIds.Length);

            // Form the GetItem request.
            GetItemType getItemRequest = new GetItemType();
            getItemRequest.ItemShape = new ItemResponseShapeType();
            getItemRequest.ItemShape.BaseShape = DefaultShapeNamesType.Default;
            getItemRequest.ItemIds = itemIds;

            PathToUnindexedFieldType itemBody = new PathToUnindexedFieldType();
            itemBody.FieldURI = UnindexedFieldURIType.itemBody;

            PathToUnindexedFieldType calendarTimeZone = new PathToUnindexedFieldType();
            calendarTimeZone.FieldURI = UnindexedFieldURIType.calendarTimeZone;

            PathToUnindexedFieldType calendarRecurrence = new PathToUnindexedFieldType();
            calendarRecurrence.FieldURI = UnindexedFieldURIType.calendarRecurrence;

            PathToUnindexedFieldType calendarFirstOccurrence = new PathToUnindexedFieldType();
            calendarFirstOccurrence.FieldURI = UnindexedFieldURIType.calendarFirstOccurrence;

            PathToUnindexedFieldType calendarLastOccurrence = new PathToUnindexedFieldType();
            calendarLastOccurrence.FieldURI = UnindexedFieldURIType.calendarLastOccurrence;

            getItemRequest.ItemShape.AdditionalProperties = new PathToUnindexedFieldType[]
			                                      	{
			                                      		itemBody, 
			                                      		calendarTimeZone,
			                                      		calendarRecurrence,
                                                        calendarFirstOccurrence,
                                                        calendarLastOccurrence
			                                      	};

            GetItemResponseType getItemResponse = Service.GetItem(getItemRequest);

            foreach (ItemInfoResponseMessageType responseMessage in getItemResponse.ResponseMessages.Items)
            {
                if (responseMessage.ResponseClass == ResponseClassType.Success &&
                    responseMessage.Items.Items != null &&
                    responseMessage.Items.Items.Length > 0)
                {
                    calendarItems.Add((CalendarItemType)responseMessage.Items.Items[0]);
                }
            }

            return calendarItems.ToArray();
        }

        protected virtual IEnumerable<Appointment> CreateAppointmentsFromCalendarItem(RadScheduler owner, CalendarItemType calendarItem)
        {
            Appointment calendarAppointment = new Appointment();
            calendarAppointment.ID = calendarItem.ItemId.Id;
            calendarAppointment.Subject = calendarItem.Subject;
            calendarAppointment.Start = calendarItem.Start;
            calendarAppointment.End = calendarItem.End;
            calendarAppointment.Owner = owner;

            calendarAppointment.Attributes[ExchangeIdAttribute] = calendarItem.ItemId.Id;
            calendarAppointment.Attributes[ExchangeChangeKeyAttribute] = calendarItem.ItemId.ChangeKey;

            List<Appointment> instances = new List<Appointment>();
            instances.Add(calendarAppointment);

            if (calendarItem.Recurrence != null)
            {
                ProcessRecurringCalendarItem(calendarAppointment, calendarItem, owner, instances);
            }

            return instances;
        }

        protected ExchangeServiceBinding GetExchangeBinding()
        {
            ExchangeServiceBinding binding = new ExchangeServiceBinding();

            ServicePointManager.ServerCertificateValidationCallback = ValidateCertificate;

            binding.Credentials = new NetworkCredential(Username, Password, Domain);
            binding.Url = ServerUrl;

            return binding;
        }

        protected virtual bool ValidateCertificate(Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None)
                return true;

            return false;
        }

        private void CreateAttachment(ItemIdType itemId)
        {
            FileAttachmentType attachment = new FileAttachmentType();
            attachment.Content = Encoding.ASCII.GetBytes("I'm an attachment");
            attachment.Name = "Attached.txt";
            attachment.ContentType = "text/plain";

            CreateAttachmentType request = new CreateAttachmentType();
            request.ParentItemId = itemId;
            request.Attachments = new AttachmentType[] { attachment };

            CreateAttachmentResponseType response = Service.CreateAttachment(request);
            AttachmentInfoResponseMessageType responseMessage = (AttachmentInfoResponseMessageType)response.ResponseMessages.Items[0];

            if (responseMessage.ResponseCode != ResponseCodeType.NoError)
            {
                throw new Exception("Error trying to create attachment. " +
                       "Response code:" + responseMessage.ResponseCode.ToString());
            }
        }

        private void DeleteItem(BaseItemIdType itemId)
        {
            DeleteItemType deleteItemRequest = new DeleteItemType();
            deleteItemRequest.DeleteType = DisposalType.HardDelete;
            deleteItemRequest.AffectedTaskOccurrences = AffectedTaskOccurrencesType.SpecifiedOccurrenceOnly;
            deleteItemRequest.AffectedTaskOccurrencesSpecified = true;
            deleteItemRequest.SendMeetingCancellations = CalendarItemCreateOrDeleteOperationType.SendToNone;
            deleteItemRequest.SendMeetingCancellationsSpecified = true;

            deleteItemRequest.ItemIds = new BaseItemIdType[] { itemId };

            DeleteItemResponseType response = Service.DeleteItem(deleteItemRequest);
            ResponseMessageType responseMessage = response.ResponseMessages.Items[0];

            if (responseMessage.ResponseCode != ResponseCodeType.NoError)
            {
                throw new Exception("DeleteItem failed with response code " + responseMessage.ResponseCode);
            }
        }

        private void RemoveRecurrenceExceptions(Appointment appointmentToUpdate)
        {
            ItemChangeType itemUpdates = new ItemChangeType();

            ItemIdType itemId = new ItemIdType();
            itemId.Id = appointmentToUpdate.Attributes[ExchangeIdAttribute];
            itemId.ChangeKey = appointmentToUpdate.Attributes[ExchangeChangeKeyAttribute];

            itemUpdates.Item = itemId;

            PathToUnindexedFieldType deletedOccurrencesPath = new PathToUnindexedFieldType();
            deletedOccurrencesPath.FieldURI = UnindexedFieldURIType.calendarRecurrence;

            DeleteItemFieldType deletedOccurrencesUpdate = new DeleteItemFieldType();
            deletedOccurrencesUpdate.Item = deletedOccurrencesPath;

            // To reset the deleted and modified occurrences we must
            // remove the recurrence rule and then immediately restore it
            itemUpdates.Updates = new ItemChangeDescriptionType[] { deletedOccurrencesUpdate, GetRecurrenceUpdate(appointmentToUpdate) };
            UpdateCalendarItem(new ItemChangeType[] { itemUpdates });
        }

        private void SynchronizeDeletedOccurrences(Appointment apt)
        {
            RecurrenceRule rrule;
            if (!RecurrenceRule.TryParse(apt.RecurrenceRule, out rrule))
            {
                return;
            }

            ItemIdType itemId = new ItemIdType();
            itemId.Id = apt.Attributes[ExchangeIdAttribute];
            itemId.ChangeKey = apt.Attributes[ExchangeChangeKeyAttribute];
            CalendarItemType calendarItem = GetCalendarItems(new ItemIdType[] { itemId })[0];

            List<DateTime> existingExceptions = new List<DateTime>();
            if (calendarItem.ModifiedOccurrences != null)
            {
                foreach (CalendarItemType modifiedOccurrence in GetModifiedOccurrences(calendarItem))
                {
                    existingExceptions.Add(modifiedOccurrence.OriginalStart);
                }
            }

            if (calendarItem.DeletedOccurrences != null)
            {
                foreach (DeletedOccurrenceInfoType deletedOccurrence in calendarItem.DeletedOccurrences)
                {
                    existingExceptions.Add(deletedOccurrence.Start);
                }
            }

            foreach (DateTime recurrenceException in rrule.Exceptions)
            {
                // Search in ModifiedOccurrences and DeletedOccurrences for this exception.	
                if (existingExceptions.Contains(recurrenceException))
                {
                    continue;
                }

                // If it is not found in either, delete the occurrence.
                int occurrenceIndex = GetOccurrenceIndex(recurrenceException, apt);
                CalendarItemType occurrenceItem = GetOccurrenceItem(apt, occurrenceIndex);
                DeleteItem(occurrenceItem.ItemId);
            }
        }

        private IEnumerable<Appointment> GetAppointments(RadScheduler owner, ItemIdType[] itemIdsArray)
        {
            IList<CalendarItemType> calendarItems = GetCalendarItems(itemIdsArray);
            List<Appointment> appointments = new List<Appointment>(calendarItems.Count);
            foreach (CalendarItemType item in calendarItems)
            {
                appointments.AddRange(CreateAppointmentsFromCalendarItem(owner, item));
            }

            return appointments;
        }

        private static RecurrenceType CreateRecurrence(RecurrenceRule schedulerRule, RadScheduler owner)
        {
            RecurrenceType recurrence = new RecurrenceType();
            recurrence.Item = RecurrencePatternBaseType.CreateFromSchedulerRecurrencePattern(schedulerRule.Pattern);
            recurrence.Item1 = RecurrenceRangeBaseType.CreateFromSchedulerRecurrenceRule(schedulerRule, owner);

            return recurrence;
        }

        private void InsertRecurrenceException(RadScheduler owner, Appointment appointmentToInsert, DateTime exceptionDate)
        {
            Appointment master = owner.Appointments.FindByID(appointmentToInsert.RecurrenceParentID);
            int occurrenceIndex = GetOccurrenceIndex(exceptionDate, master);
            CalendarItemType occurrenceItem = GetOccurrenceItem(master, occurrenceIndex);
            occurrenceItem.MeetingTimeZone = GetTimeZone(appointmentToInsert.Owner.TimeZoneOffset);

            // Update the occurrence
            ItemChangeType itemUpdates = GetAppointmentChanges(appointmentToInsert);
            itemUpdates.Item = occurrenceItem.ItemId;
            ItemChangeType[] changes = new ItemChangeType[] { itemUpdates };

            UpdateCalendarItem(changes);
        }

        private void UpdateCalendarItem(ItemChangeType[] changes)
        {
            UpdateItemType updateItemRequest = new UpdateItemType();
            updateItemRequest.ConflictResolution = ConflictResolutionType.AlwaysOverwrite;
            updateItemRequest.ItemChanges = changes;
            updateItemRequest.SendMeetingInvitationsOrCancellations = CalendarItemUpdateOperationType.SendToNone;
            updateItemRequest.SendMeetingInvitationsOrCancellationsSpecified = true;

            UpdateItemResponseType response = Service.UpdateItem(updateItemRequest);
            ResponseMessageType responseMessage = response.ResponseMessages.Items[0];

            if (responseMessage.ResponseCode != ResponseCodeType.NoError)
            {
                throw new Exception("UpdateItem failed with response code " + responseMessage.ResponseCode);
            }
        }

        private CalendarItemType GetOccurrenceItem(Appointment master, int index)
        {
            ItemIdType masterItemId = new ItemIdType();
            masterItemId.Id = master.Attributes[ExchangeIdAttribute];
            masterItemId.ChangeKey = master.Attributes[ExchangeChangeKeyAttribute];

            OccurrenceItemIdType occurrenceItemId = new OccurrenceItemIdType();
            occurrenceItemId.RecurringMasterId = masterItemId.Id;
            occurrenceItemId.InstanceIndex = index;

            PathToUnindexedFieldType calendarItemTypePath = new PathToUnindexedFieldType();
            calendarItemTypePath.FieldURI = UnindexedFieldURIType.calendarCalendarItemType;

            GetItemType getItemRequest = new GetItemType();
            getItemRequest.ItemShape = new ItemResponseShapeType();
            getItemRequest.ItemShape.BaseShape = DefaultShapeNamesType.IdOnly;
            getItemRequest.ItemShape.AdditionalProperties = new BasePathToElementType[] { calendarItemTypePath };
            getItemRequest.ItemIds = new BaseItemIdType[] { masterItemId, occurrenceItemId };

            GetItemResponseType getItemResponse = Service.GetItem(getItemRequest);

            CalendarItemType occurrenceItem = null;
            foreach (ItemInfoResponseMessageType getItemResponseMessage in getItemResponse.ResponseMessages.Items)
            {
                if (getItemResponseMessage.ResponseClass == ResponseClassType.Success &&
                    getItemResponseMessage.Items.Items != null &&
                    getItemResponseMessage.Items.Items.Length > 0)
                {
                    occurrenceItem = (CalendarItemType)getItemResponseMessage.Items.Items[0];
                }
            }

            if (occurrenceItem == null)
            {
                throw new Exception("Unable to find occurrence");
            }

            return occurrenceItem;
        }

        private static int GetOccurrenceIndex(DateTime occurrenceStart, Appointment master)
        {
            RecurrenceRule rrule;
            RecurrenceRule.TryParse(master.RecurrenceRule, out rrule);

            // Exceptions must be counted too
            rrule.Exceptions.Clear();

            int index = 1;
            foreach (DateTime occ in rrule.Occurrences)
            {
                if (occ == occurrenceStart)
                {
                    break;
                }

                index++;
            }
            return index;
        }

        private static SetItemFieldType GetSubjectUpdate(Appointment apt)
        {
            SetItemFieldType subjectUpdate = new SetItemFieldType();

            PathToUnindexedFieldType subjectPath = new PathToUnindexedFieldType();
            subjectPath.FieldURI = UnindexedFieldURIType.itemSubject;

            CalendarItemType subjectData = new CalendarItemType();
            subjectData.Subject = apt.Subject;

            subjectUpdate.Item = subjectPath;
            subjectUpdate.Item1 = subjectData;
            return subjectUpdate;
        }

        private static SetItemFieldType GetStartUpdate(Appointment apt)
        {
            SetItemFieldType startUpdate = new SetItemFieldType();

            PathToUnindexedFieldType startPath = new PathToUnindexedFieldType();
            startPath.FieldURI = UnindexedFieldURIType.calendarStart;

            CalendarItemType startData = new CalendarItemType();
            startData.Start = apt.Start;
            startData.StartSpecified = true;

            startUpdate.Item = startPath;
            startUpdate.Item1 = startData;
            return startUpdate;
        }

        private static SetItemFieldType GetEndUpdate(Appointment apt)
        {
            SetItemFieldType endUpdate = new SetItemFieldType();

            PathToUnindexedFieldType endPath = new PathToUnindexedFieldType();
            endPath.FieldURI = UnindexedFieldURIType.calendarEnd;

            CalendarItemType endData = new CalendarItemType();
            endData.End = apt.End;
            endData.EndSpecified = true;

            endUpdate.Item = endPath;
            endUpdate.Item1 = endData;
            return endUpdate;
        }

        private static SetItemFieldType GetRecurrenceUpdate(Appointment apt)
        {
            SetItemFieldType recurrenceUpdate = new SetItemFieldType();

            PathToUnindexedFieldType recurrencePath = new PathToUnindexedFieldType();
            recurrencePath.FieldURI = UnindexedFieldURIType.calendarRecurrence;

            CalendarItemType recurrenceData = new CalendarItemType();
            RecurrenceRule rrule;
            RecurrenceRule.TryParse(apt.RecurrenceRule, out rrule);
            if (rrule != null && rrule.Pattern.Frequency != RecurrenceFrequency.Hourly)
            {
                recurrenceData.Recurrence = CreateRecurrence(rrule, apt.Owner);
            }

            recurrenceUpdate.Item = recurrencePath;
            recurrenceUpdate.Item1 = recurrenceData;
            return recurrenceUpdate;
        }

        private void ProcessRecurringCalendarItem(Appointment targetAppointment, CalendarItemType calendarItem, RadScheduler owner, ICollection<Appointment> instances)
        {
            targetAppointment.RecurrenceState = RecurrenceState.Master;

            RecurrencePattern pattern = calendarItem.Recurrence.Item.ConvertToRecurrencePattern();

            RecurrenceRange range = calendarItem.Recurrence.Item1.ConvertToRecurrenceRange();
            range.EventDuration = targetAppointment.Duration;
            range.Start = targetAppointment.Start;

            RecurrenceRule rrule = RecurrenceRule.FromPatternAndRange(pattern, range);

            if (calendarItem.ModifiedOccurrences != null)
            {
                foreach (CalendarItemType modifiedOccurrence in GetModifiedOccurrences(calendarItem))
                {
                    foreach (Appointment aptException in CreateAppointmentsFromCalendarItem(owner, modifiedOccurrence))
                    {
                        aptException.RecurrenceState = RecurrenceState.Exception;
                        aptException.RecurrenceParentID = calendarItem.ItemId.Id;

                        instances.Add(aptException);
                    }

                    rrule.Exceptions.Add(modifiedOccurrence.OriginalStart);
                }
            }

            if (calendarItem.DeletedOccurrences != null)
            {
                foreach (DeletedOccurrenceInfoType occurenceInfo in calendarItem.DeletedOccurrences)
                {
                    rrule.Exceptions.Add(occurenceInfo.Start);
                }
            }

            targetAppointment.RecurrenceRule = rrule.ToString();
        }

        private IList<CalendarItemType> GetModifiedOccurrences(CalendarItemType calendarItem)
        {
            List<ItemIdType> exceptionIds = new List<ItemIdType>();
            foreach (OccurrenceInfoType occurenceInfo in calendarItem.ModifiedOccurrences)
            {
                exceptionIds.Add(occurenceInfo.ItemId);
            }

            return GetCalendarItems(exceptionIds.ToArray());
        }

        protected internal List<BaseFolderIdType> FindCalendarFolderByName(string folderName)
        {
            List<BaseFolderIdType> theReturn = new List<BaseFolderIdType>();

            if (folderName == null || folderName == string.Empty)
                theReturn.Add(new DistinguishedFolderIdType() { Id = DistinguishedFolderIdNameType.calendar });
            else
            {
                // LOOKUP THE FOLDER BY ITS FRIENDLY (DISPLAY) NAME

                FindFolderType findFolder = new FindFolderType();
                findFolder.FolderShape = new FolderResponseShapeType() { BaseShape = DefaultShapeNamesType.IdOnly };
                findFolder.Traversal = FolderQueryTraversalType.Deep;

                // Identify which folder to begin your search from.
                List<DistinguishedFolderIdType> folders = new List<DistinguishedFolderIdType>();
                DistinguishedFolderIdType rootFolder = new DistinguishedFolderIdType() { Id = DistinguishedFolderIdNameType.root };
                folders.Add(rootFolder);
                findFolder.ParentFolderIds = folders.ToArray();

                // need display name property to compare against
                PathToUnindexedFieldType displayNamePath = new PathToUnindexedFieldType() { FieldURI = UnindexedFieldURIType.folderDisplayName };
                findFolder.FolderShape.AdditionalProperties = new BasePathToElementType[] { displayNamePath };

                // execute the request
                FindFolderResponseType response = Service.FindFolder(findFolder);

                // FILTER OUT THE ONE FOLDER WE'RE LOOKING FOR, AND GET ITS ID

                // Access the response message.
                ArrayOfResponseMessagesType responseMessages = response.ResponseMessages;

                List<string> CalendarFolderIDs = new List<string>();
                foreach (ResponseMessageType responseMessage in responseMessages.Items)
                {
                    if (responseMessage is FindFolderResponseMessageType)
                    {
                        FindFolderResponseMessageType ffrmt = (responseMessage as FindFolderResponseMessageType);
                        FindFolderParentType ffpt = ffrmt.RootFolder;

                        foreach (BaseFolderType item in ffpt.Folders)
                        {
                            if (item is CalendarFolderType)
                            {
                                CalendarFolderType calendarFolder = item as CalendarFolderType;
                                if (calendarFolder != null)
                                {
                                    if (calendarFolder.DisplayName.ToLower() == CustomCalendarName.ToLower())
                                    {
                                        FolderIdType calID = calendarFolder.FolderId;
                                        theReturn.Add(calID);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return theReturn;
        }
    }
