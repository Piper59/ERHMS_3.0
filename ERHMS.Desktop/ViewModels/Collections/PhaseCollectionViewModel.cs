﻿using ERHMS.Desktop.Data;
using ERHMS.Desktop.Infrastructure.ViewModels;
using ERHMS.Domain;
using System.Collections.Generic;
using System.Linq;

namespace ERHMS.Desktop.ViewModels.Collections
{
    public class PhaseCollectionViewModel : ViewModel
    {
        public class ItemViewModel : SelectableViewModel
        {
            public Phase Value { get; }
            public CoreProject CoreProject { get; }
            public IReadOnlyCollection<CoreView> CoreViews { get; }

            public ItemViewModel(Phase value)
            {
                Value = value;
                CoreProject = value.ToCoreProject();
                CoreViews = CoreView.GetInstances(value).ToList();
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return obj is ItemViewModel item && Value == item.Value;
            }
        }

        private readonly List<ItemViewModel> items;
        public CustomCollectionView<ItemViewModel> Items { get; }

        public PhaseCollectionViewModel()
        {
            items = new List<ItemViewModel>
            {
                new ItemViewModel(Phase.PreDeployment),
                new ItemViewModel(Phase.Deployment),
                new ItemViewModel(Phase.PostDeployment)
            };
            Items = new CustomCollectionView<ItemViewModel>(items);
        }
    }
}
