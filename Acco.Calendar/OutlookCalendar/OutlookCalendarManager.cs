﻿//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//
using Microsoft.Office.Interop.Outlook;
//
using Acco.Calendar.Person;
using Acco.Calendar.Event;
using Acco.Calendar.Location;

namespace Acco.Calendar.Manager
{

    public sealed class OutlookCalendarManager : ICalendarManager
    {
        #region Variables
        private Application OutlookApplication { get; set; }
        private NameSpace MapiNameSpace { get; set; }
        private MAPIFolder CalendarFolder { get; set; }
        #endregion

        #region Singleton directives
        private static readonly OutlookCalendarManager instance = new OutlookCalendarManager();
        // hidden constructor
        private OutlookCalendarManager() { Initialize(); }



        public static OutlookCalendarManager Instance { get { return instance; } }
        #endregion

        private void Initialize()
        {
            OutlookApplication = new Application();
            MapiNameSpace = OutlookApplication.GetNamespace("MAPI");
            CalendarFolder = MapiNameSpace.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
        }

        public bool Push(GenericCalendar calendar)
        {
            bool result = true;
            // TODO: set various infos here
            //
            foreach(GenericEvent evt in calendar.Events)
            {

            }
            //
            return result;
        }

        public GenericCalendar Pull()
        {
            GenericCalendar myCalendar = new GenericCalendar();
            //
            myCalendar.Id = CalendarFolder.EntryID;
            myCalendar.Name = CalendarFolder.Name;
            myCalendar.Creator = new GenericPerson();
            myCalendar.Creator.Email = CalendarFolder.Store.DisplayName;
            //
            myCalendar.Events = new List<GenericEvent>();
            //
            Items evts = CalendarFolder.Items;
            evts.IncludeRecurrences = true;
            evts.Sort("[Start]");
            string filter = "[Start] >= '"
                + DateTime.Now.ToString("g")
                + "' AND [End] <= '"
                + DateTime.Now.Add(new TimeSpan(30 /*days*/, 0 /* hours */, 0 /*minutes*/, 0 /* seconds*/)).ToString("g") + "'";
            evts = evts.Restrict(filter);
            foreach (AppointmentItem evt in evts)
            {
                //
                GenericEvent myEvt = new GenericEvent();
                // Start
                myEvt.Start = evt.Start;
                // End
                if(!evt.AllDayEvent)
                { 
                    myEvt.End = evt.End;
                }
                // Description
                myEvt.Description = evt.Subject;
                // Summary
                myEvt.Summary = evt.Body;
                // Location
                myEvt.Location = new GenericLocation();
                myEvt.Location.Name = evt.Location;
                // Creator and organizer are the same person.
                // Creator
                myEvt.Creator = new GenericPerson();
                myEvt.Creator.Email = evt.GetOrganizer().Address;
                myEvt.Creator.Name = evt.GetOrganizer().Name;
                // Organizer
                myEvt.Organizer = new GenericPerson();
                myEvt.Organizer.Email = evt.GetOrganizer().Address;
                myEvt.Organizer.Name = evt.GetOrganizer().Name;
                // Attendees
                myEvt.Attendees = new List<GenericPerson>();
                string[] requiredAttendeesEmails = null;
                if(evt.RequiredAttendees != null)
                { 
                    requiredAttendeesEmails = evt.RequiredAttendees.Split(';');
                }
                string[] optionalAttendeesEmails = null;
                if(evt.OptionalAttendees != null)
                { 
                    optionalAttendeesEmails = evt.OptionalAttendees.Split(';');
                }
                //
                if(requiredAttendeesEmails != null)
                { 
                    foreach(string attendeeEmail in requiredAttendeesEmails)
                    {
                        myEvt.Attendees.Add(new GenericPerson 
                        {
                            Email = attendeeEmail.Trim()
                        });
                    }
                }
                //
                if(optionalAttendeesEmails != null)
                { 
                    foreach(string optionalAttendee in optionalAttendeesEmails)
                    {
                        myEvt.Attendees.Add(new GenericPerson
                        {
                            Email = optionalAttendee.Trim()
                        });
                    }
                }
                // Recurrency (it seems that on fucking microsoft outlook recurrency is from an alien planet) 
                //if (evt.IsRecurring)
                //{
                //    myEvt.Recurrency = new GenericRecurrency();
                //    //
                //    RecurrencePattern rp = evt.GetRecurrencePattern();
                //    OutlookRecurrency temporaryRecurrency = new OutlookRecurrency();
                //    temporaryRecurrency.Expiry = rp.PatternEndDate;
                //    temporaryRecurrency.Parse(rp);
                //    // this is bad, but I don't know how to do otherwise.
                //    myEvt.Recurrency.Days = temporaryRecurrency.Days;
                //    myEvt.Recurrency.Expiry = temporaryRecurrency.Expiry;
                //    myEvt.Recurrency.Type = temporaryRecurrency.Type;
                //    //
                //}
                // add it to calendar events.
                myCalendar.Events.Add(myEvt);
            }
            //
            return myCalendar;
        }

        public async Task<bool> PushAsync(GenericCalendar calendar)
        {
            return Push(calendar);
        }

        public async Task<GenericCalendar> PullAsync()
        {
            return Pull();
        }
    }
}
