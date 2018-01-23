﻿using ERHMS.Domain;
using ERHMS.Presentation.Commands;
using ERHMS.Presentation.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ERHMS.Presentation.ViewModels
{
    public class IncidentNoteListViewModel : DocumentViewModel
    {
        public class IncidentNoteListChildViewModel : ListViewModel<IncidentNote>
        {
            public Incident Incident { get; private set; }

            public IncidentNoteListChildViewModel(IServiceManager services, Incident incident)
                : base(services)
            {
                Incident = incident;
                Refresh();
            }

            protected override IEnumerable<IncidentNote> GetItems()
            {
                return Context.IncidentNotes.SelectByIncidentId(Incident.IncidentId).OrderByDescending(incidentNote => incidentNote.Date);
            }
        }

        public Incident Incident { get; private set; }
        public IncidentNoteListChildViewModel IncidentNotes { get; private set; }

        private string content;
        [DirtyCheck]
        public string Content
        {
            get { return content; }
            set { SetProperty(nameof(Content), ref content, value); }
        }

        public ICommand SaveCommand { get; private set; }

        public IncidentNoteListViewModel(IServiceManager services, Incident incident)
            : base(services)
        {
            Title = "Notes";
            Incident = incident;
            IncidentNotes = new IncidentNoteListChildViewModel(services, incident);
            SaveCommand = new Command(Save, CanSave);
        }

        public bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Content);
        }

        public void Save()
        {
            Context.IncidentNotes.Save(new IncidentNote(true)
            {
                IncidentId = Incident.IncidentId,
                Content = Content,
                Date = DateTime.Now
            });
            Services.Data.Refresh(typeof(IncidentNote));
            Content = "";
            Dirty = false;
        }

        public override void Dispose()
        {
            IncidentNotes.Dispose();
            base.Dispose();
        }
    }
}
