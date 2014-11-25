using System.Collections.Generic;
using Lucene.Net.Documents;
using Lucene.Net.Search;

namespace Aucent.MAX.AXE.XBRLParser.Searching.Indexing.Lucene
{
    ///<summary>
    ///</summary>
    public class LuceneLibrarySearchResult : ILibrarySearchResult
    {
        private readonly Hits _innerHits;
        private List<ILibrarySearchResultItem> _cachedItems;

        ///<summary>
        ///</summary>
        public LuceneLibrarySearchResult(Hits _innerHits, string allowOnlyResultsWhosePathStartsWith, double minScoreAllowed)
        {
            this._innerHits = _innerHits;

            List<ILibrarySearchResultItem> toBeDeleted = new List<ILibrarySearchResultItem>();

            foreach(ILibrarySearchResultItem item in Items)
            {
                if( !string.IsNullOrEmpty(allowOnlyResultsWhosePathStartsWith) && (string.IsNullOrEmpty(item.TaxonomyPath) || !item.TaxonomyPath.StartsWith(allowOnlyResultsWhosePathStartsWith)))
                {
                    toBeDeleted.Add(item);
                }
                else if(item.SearchScore < minScoreAllowed)
                {
                    toBeDeleted.Add(item);
                }
            }

            foreach(ILibrarySearchResultItem tbd in toBeDeleted)
            {
                items.Remove(tbd);
            }
        }

        ///<summary>
        ///</summary>
        public int Count
        {
            get
            {
                return Items.Length;
            }
        }

        private List<ILibrarySearchResultItem> items
        {
            get
            {
                if (_cachedItems == null)
                {
                    lock(this)
                    {
                        if (_cachedItems == null)
                        {
                            _cachedItems = new List<ILibrarySearchResultItem>();

                            for (int i = 0; i < _innerHits.Length(); i++)
                            {
                                _cachedItems.Add(new LuceneLibrarySearchResultItem(_innerHits.Doc(i), _innerHits.Score(i), _innerHits.Id(i)));
                            }
                        }
                    }
                }

                return _cachedItems;
            }
        }

        ///<summary>
        ///</summary>
        public ILibrarySearchResultItem[] Items
        {
            get
            {
                return items.ToArray();
            }
        }

        ///<summary>
        ///</summary>
        public IEnumerator<ILibrarySearchResultItem> Iterator()
        {

            return items.GetEnumerator();
        }
    }

    ///<summary>
    ///</summary>
    public class LuceneLibrarySearchResultItem : ILibrarySearchResultItem
    {
        private readonly Document _innerHit;
        private readonly float _score;
        private readonly int _resultId;

        ///<summary>
        ///</summary>
        public LuceneLibrarySearchResultItem(Document _innerHit, float _score, int _resultId)
        {
            this._innerHit = _innerHit;
            this._resultId = _resultId;
            this._score = _score;
        }

        ///<summary>
        ///</summary>
        public string ElementID
        {
            get { return _innerHit.Get(LuceneNodeIndexer.ELEMENTID_FIELD); }
        }

        ///<summary>
        ///</summary>
        public string TaxonomyPath
        {
            get { return _innerHit.Get(LuceneNodeIndexer.PATH_FIELD); }
        }

        ///<summary>
        ///</summary>
        public double ElementOrder
        {
            get { return double.Parse(_innerHit.Get(LuceneNodeIndexer.ORDER_FIELD)); }
        }

        ///<summary>
        ///</summary>
        public float SearchScore
        {
            get { return _score; }
        }

        ///<summary>
        ///</summary>
        public string Label
        {
            get { return _innerHit.Get(LuceneNodeIndexer.LABEL_FIELD); }
        }

        ///<summary>
        ///</summary>
        public int ResultId
        {
            get { return _resultId; }
        }
    }
}
