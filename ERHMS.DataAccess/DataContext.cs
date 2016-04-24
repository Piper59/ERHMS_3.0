﻿using Epi;
using ERHMS.EpiInfo;
using ERHMS.EpiInfo.DataAccess;
using System.Collections.Generic;
using System.Linq;
using Project = ERHMS.EpiInfo.Project;
using Template = ERHMS.EpiInfo.Template;

namespace ERHMS.DataAccess
{
    public class DataContext
    {
        public Project Project { get; private set; }
        public IDataDriver Driver { get; private set; }
        public CodeRepository Prefixes { get; private set; }
        public CodeRepository Suffixes { get; private set; }
        public CodeRepository Genders { get; private set; }
        public CodeRepository States { get; private set; }
        public ResponderRepository Responders { get; private set; }
        public IncidentRepository Incidents { get; private set; }
        public LocationRepository Locations { get; private set; }
        public RosterRepository Rosters { get; private set; }
        public AssignmentRepository Assignments { get; private set; }
        public ViewLinkRepository ViewLinks { get; private set; }
        public PgmLinkRepository PgmLinks { get; private set; }
        public CanvasLinkRepository CanvasLinks { get; private set; }

        public DataContext(Project project)
        {
            Log.Current.DebugFormat("Opening data context: {0}", project.FilePath);
            Project = project;
            Driver = DataDriverFactory.CreateDataDriver(project);
            Prefixes = new CodeRepository(Driver, "codeprefix1", "prefix", false);
            Suffixes = new CodeRepository(Driver, "codesuffix1", "suffix", false);
            Genders = new CodeRepository(Driver, "codegender1", "gender", false);
            States = new CodeRepository(Driver, "codestate1", "state", true);
            Responders = new ResponderRepository(Driver, project);
            Incidents = new IncidentRepository(Driver);
            Locations = new LocationRepository(Driver);
            Rosters = new RosterRepository(Driver);
            Assignments = new AssignmentRepository(Driver);
            ViewLinks = new ViewLinkRepository(Driver);
            PgmLinks = new PgmLinkRepository(Driver);
            CanvasLinks = new CanvasLinkRepository(Driver);
        }

        private DataPredicate GetLinkPredicate(string incidentId)
        {
            DataParameterCollection parameters = new DataParameterCollection(Driver);
            parameters.AddByValue(incidentId);
            string sql = parameters.Format("IncidentId = {0}");
            return new DataPredicate(sql, parameters);
        }

        public IEnumerable<View> GetViews()
        {
            return Project.GetViews();
        }

        public IEnumerable<Template> GetTemplates(TemplateLevel? level = null)
        {
            if (level.HasValue)
            {
                return Template.GetByLevel(level.Value);
            }
            else
            {
                return Template.GetAll();
            }
        }

        public IEnumerable<Pgm> GetPgms()
        {
            return Project.GetPgms();
        }

        public IEnumerable<Canvas> GetCanvases()
        {
            return Project.GetCanvases();
        }

        public IEnumerable<View> GetLinkedViews(string incidentId)
        {
            ICollection<int> viewIds = ViewLinks.Select(GetLinkPredicate(incidentId))
                .Select(viewLink => viewLink.ViewId)
                .ToList();
            return GetViews().Where(view => viewIds.Contains(view.Id));
        }

        public IEnumerable<Pgm> GetLinkedPgms(string incidentId)
        {
            ICollection<int> pgmIds = PgmLinks.Select(GetLinkPredicate(incidentId))
                .Select(pgmLink => pgmLink.PgmId)
                .ToList();
            return GetPgms().Where(pgm => pgmIds.Contains(pgm.PgmId));
        }

        public IEnumerable<Canvas> GetLinkedCanvases(string incidentId)
        {
            ICollection<int> canvasIds = CanvasLinks.Select(GetLinkPredicate(incidentId))
                .Select(canvasLink => canvasLink.CanvasId)
                .ToList();
            return GetCanvases().Where(canvas => canvasIds.Contains(canvas.CanvasId));
        }

        public IEnumerable<View> GetUnlinkedViews()
        {
            ICollection<int> viewIds = ViewLinks.Select()
                .Select(viewLink => viewLink.ViewId)
                .ToList();
            return GetViews().Where(view => !viewIds.Contains(view.Id));
        }

        public IEnumerable<Pgm> GetUnlinkedPgms()
        {
            ICollection<int> pgmIds = PgmLinks.Select()
                .Select(pgmLink => pgmLink.PgmId)
                .ToList();
            return GetPgms().Where(pgm => !pgmIds.Contains(pgm.PgmId));
        }

        public IEnumerable<Canvas> GetUnlinkedCanvases()
        {
            ICollection<int> canvasIds = CanvasLinks.Select()
                .Select(canvasLink => canvasLink.CanvasId)
                .ToList();
            return GetCanvases().Where(canvas => !canvasIds.Contains(canvas.CanvasId));
        }
    }
}
