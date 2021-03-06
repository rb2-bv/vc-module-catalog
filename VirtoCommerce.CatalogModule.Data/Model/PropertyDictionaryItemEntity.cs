using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class PropertyDictionaryItemEntity : Entity
    {
        public PropertyDictionaryItemEntity()
        {
            DictionaryItemValues = new NullCollection<PropertyDictionaryValueEntity>();
        }

        [StringLength(512)]
        [Required]
        [Index("IX_AliasAndPropertyId", 1, IsUnique = true)]
        public string Alias { get; set; }

        public int SortOrder { get; set; }

        #region Navigation Properties
        [Index("IX_AliasAndPropertyId", 2, IsUnique = true)]
        public string PropertyId { get; set; }
        public virtual PropertyEntity Property { get; set; }

        public virtual ObservableCollection<PropertyDictionaryValueEntity> DictionaryItemValues { get; set; }
        #endregion

        public virtual PropertyDictionaryItem ToModel(PropertyDictionaryItem propDictItem)
        {
            if (propDictItem == null)
            {
                throw new ArgumentNullException(nameof(propDictItem));
            }
            propDictItem.Id = Id;
            propDictItem.Alias = Alias;
            propDictItem.SortOrder = SortOrder;
            propDictItem.PropertyId = PropertyId;
            propDictItem.LocalizedValues = DictionaryItemValues.Select(x => x.ToModel(AbstractTypeFactory<PropertyDictionaryItemLocalizedValue>.TryCreateInstance())).ToList();

            return propDictItem;
        }

        public virtual PropertyDictionaryItemEntity FromModel(PropertyDictionaryItem propDictItem, PrimaryKeyResolvingMap pkMap)
        {
            if (propDictItem == null)
            {
                throw new ArgumentNullException(nameof(propDictItem));
            }
            pkMap.AddPair(propDictItem, this);

            Id = propDictItem.Id;
            Alias = propDictItem.Alias;
            SortOrder = propDictItem.SortOrder;
            PropertyId = propDictItem.PropertyId;
            if (propDictItem.LocalizedValues != null)
            {
                DictionaryItemValues = new ObservableCollection<PropertyDictionaryValueEntity>(propDictItem.LocalizedValues.Select(x => AbstractTypeFactory<PropertyDictionaryValueEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }
            return this;
        }

        //Left only for backward compatibility when dictionary items used to save within property
        [Obsolete]
        public static IEnumerable<PropertyDictionaryItemEntity> FromModels(IEnumerable<PropertyDictionaryValue> dictValues, PrimaryKeyResolvingMap pkMap)
        {
            if (dictValues == null)
            {
                throw new ArgumentNullException(nameof(dictValues));
            }
            //Need to group all incoming DictValues by alias to convert flat list of values to data model structure  
            foreach (var dictItemGroup in dictValues.GroupBy(x => x.Alias))
            {
                var dictItemEntity = AbstractTypeFactory<PropertyDictionaryItemEntity>.TryCreateInstance();
                dictItemEntity.Id = dictItemGroup.First().ValueId ?? dictItemGroup.First().Id;
                dictItemEntity.Alias = dictItemGroup.Key;
                dictItemEntity.PropertyId = dictItemGroup.First().PropertyId;

                dictItemEntity.DictionaryItemValues = new ObservableCollection<PropertyDictionaryValueEntity>();
                foreach (var dictValue in dictItemGroup)
                {
                    var dictValueEntity = AbstractTypeFactory<PropertyDictionaryValueEntity>.TryCreateInstance();
                    pkMap.AddPair(dictValue, dictValueEntity);
                    dictValueEntity.Id = dictValue.Id;
                    dictValueEntity.Locale = dictValue.LanguageCode;
                    dictValueEntity.Value = dictValue.Value ?? dictValue.Alias;
                    dictItemEntity.DictionaryItemValues.Add(dictValueEntity);
                }
                yield return dictItemEntity;
            }
        }


        public virtual void Patch(PropertyDictionaryItemEntity target)
        {
            target.Alias = Alias;
            target.SortOrder = SortOrder;
            if (!DictionaryItemValues.IsNullCollection())
            {
                var comparer = AnonymousComparer.Create((PropertyDictionaryValueEntity x) => x.Value + '|' + x.Locale);
                DictionaryItemValues.Patch(target.DictionaryItemValues, comparer, (sourceDictItem, targetDictItem) => sourceDictItem.Patch(targetDictItem));
            }
        }

    }
}
