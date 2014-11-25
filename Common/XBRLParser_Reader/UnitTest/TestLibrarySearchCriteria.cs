using Aucent.MAX.AXE.XBRLParser;
using Aucent.MAX.AXE.XBRLParser.Searching;
using NUnit.Framework;

#if UNITTEST
namespace Aucent.MAX.AXE.XBRLParser.UnitTests
{
    /// <summary>
    /// </summary>
    [TestFixture]
    public class TestLibrarySearchCriteria
    {
        private static void AssertFullResetState(LibrarySearchCriteria crit)
        {
            Assert.AreEqual(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels, LibrarySearchCriteria.SearchableTextField.Definitions }, crit.TextFieldsToSearch, "Post-reset TextFieldsToSearch");
            Assert.AreEqual(new Element.BalanceType[] { }, crit.BalanceTypes, "Post-reset BalanceTypes");
            Assert.AreEqual(new string[] { }, crit.ProhibitedWords, "Post-reset WithoutWords");
            Assert.AreEqual(new string[] { }, crit.OptionalWords, "Post-reset OneOrMoreWords");
            Assert.AreEqual(new string[] { }, crit.RequiredWords, "Post-reset AllWords");
            Assert.AreEqual(true, crit.IsIncludeAbstractElements, "Post-reset IsIncludeAbstractElements");
            Assert.AreEqual(true, crit.IsIncludeExtendedElements, "Post-reset IsIncludeExtendedElements");
            Assert.IsNull(crit.AncestryPath, "Pre-reset AncestryPath");
        }

        /// <summary>
        /// </summary>
        [Test]
        public void Reset()
        {
            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .AddTextFieldToSearch(LibrarySearchCriteria.SearchableTextField.Names)
                .AddBalanceType(Element.BalanceType.na)
                .AddProhibitedWord("Crapburger")
                .AddOptionalWord("Cheeseburger")
                .AddRequiredWord("Now")
                .ExcludingAbstractElements()
                .ExcludingExtendedElements()
                .MatchingWholeWords()
                .DescendingFrom("A/Path/Right/Here");

            Assert.AreEqual(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels, LibrarySearchCriteria.SearchableTextField.Definitions, LibrarySearchCriteria.SearchableTextField.Names }, crit.TextFieldsToSearch, "Pre-reset TextFieldsToSearch");
            Assert.AreEqual(new Element.BalanceType[] { Element.BalanceType.na }, crit.BalanceTypes, "Pre-reset BalanceTypes");
            Assert.AreEqual(new string[] { "Crapburger" }, crit.ProhibitedWords, "Pre-reset WithoutWords");
            Assert.AreEqual(new string[] { "Cheeseburger" }, crit.OptionalWords, "Pre-reset OneOrMoreWords");
            Assert.AreEqual(new string[] { "Now" }, crit.RequiredWords, "Pre-reset AllWords");
            Assert.AreEqual(false, crit.IsIncludeAbstractElements, "Pre-reset IsIncludeAbstractElements");
            Assert.AreEqual(false, crit.IsIncludeExtendedElements, "Pre-reset IsIncludeExtendedElements");
            Assert.AreEqual("A/Path/Right/Here", crit.AncestryPath, "Pre-reset AncestryPath");

            crit.Reset();
            AssertFullResetState(crit);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void WithOneOrMoreWordsSeparatedBySpace_Clears_And_Adds_Range()
        {
            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .WithOptionalWords("Joey");

            Assert.AreEqual(new string[] { "Joey" }, crit.OptionalWords);

            crit.WithOptionalWords("Lawrence");

            Assert.AreEqual(new string[] { "Lawrence" }, crit.OptionalWords);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void AddIncludingWord_Appends_Word()
        {
            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .AddOptionalWord("Joey")
                .AddOptionalWord("Lawrence");

            Assert.AreEqual(new string[] { "Joey", "Lawrence" }, crit.OptionalWords);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void WithAllWordsSeparatedBySpace_Clears_And_Adds_Range()
        {
            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .WithRequiredWords("Joey");

            Assert.AreEqual(new string[] { "Joey" }, crit.RequiredWords);

            crit.WithRequiredWords("Lawrence");

            Assert.AreEqual(new string[] { "Lawrence" }, crit.RequiredWords);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void AddRequiredWord_Appends_Word()
        {
            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .AddRequiredWord("Joey")
                .AddRequiredWord("Lawrence");

            Assert.AreEqual(new string[] { "Joey", "Lawrence" }, crit.RequiredWords);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void WithoutWordsSeparatedBySpace_Clears_And_Adds_Range()
        {
            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .WithProhibitedWords("Joey");

            Assert.AreEqual(new string[] { "Joey" }, crit.ProhibitedWords);

            crit.WithProhibitedWords("Lawrence");

            Assert.AreEqual(new string[] { "Lawrence" }, crit.ProhibitedWords);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void AddBannedWord_Appends_Word()
        {
            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .AddProhibitedWord("Joey")
                .AddProhibitedWord("Lawrence");

            Assert.AreEqual(new string[] { "Joey", "Lawrence" }, crit.ProhibitedWords);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void AddBalanceType_Adds_Type()
        {
            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .AddBalanceType(Element.BalanceType.credit)
                .AddBalanceType(Element.BalanceType.debit);

            Assert.AreEqual(new Element.BalanceType[] { Element.BalanceType.credit, Element.BalanceType.debit }, crit.BalanceTypes);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void WithBalanceTypes_Clears_And_Adds_Range()
        {
            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .WithBalanceTypes(new Element.BalanceType[] {Element.BalanceType.credit});

            Assert.AreEqual(new Element.BalanceType[] { Element.BalanceType.credit }, crit.BalanceTypes);

            crit.WithBalanceTypes(new Element.BalanceType[] { Element.BalanceType.debit });

            Assert.AreEqual(new Element.BalanceType[] { Element.BalanceType.debit }, crit.BalanceTypes);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void AddTextFieldToSearch_Adds_Type()
        {
            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .AddTextFieldToSearch(LibrarySearchCriteria.SearchableTextField.Names);

            Assert.AreEqual(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels, LibrarySearchCriteria.SearchableTextField.Definitions, LibrarySearchCriteria.SearchableTextField.Names }, crit.TextFieldsToSearch);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void AgainstTextFields_Clears_And_Adds_Range()
        {
            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels });

            Assert.AreEqual(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels }, crit.TextFieldsToSearch);

            crit.AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Definitions });

            Assert.AreEqual(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Definitions }, crit.TextFieldsToSearch);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void IncludingAbstractElements_And_ExcludingAbstractElements()
        {
            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .IncludingAbstractElements();

            Assert.IsTrue(crit.IsIncludeAbstractElements);

            crit.ExcludingAbstractElements();
            Assert.IsFalse(crit.IsIncludeAbstractElements);

            crit.IncludingAbstractElements();
            Assert.IsTrue(crit.IsIncludeAbstractElements);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void IncludeAbtractElements()
        {
            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .SetIncludeAbstractElements(true);

            Assert.IsTrue(crit.IsIncludeAbstractElements);

            crit.SetIncludeAbstractElements(false);
            Assert.IsFalse(crit.IsIncludeAbstractElements);

            crit.SetIncludeAbstractElements(true);
            Assert.IsTrue(crit.IsIncludeAbstractElements);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void IncludingExtendedElements_And_ExcludingExtendedElements()
        {
            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .IncludingExtendedElements();

            Assert.IsTrue(crit.IsIncludeExtendedElements);

            crit.ExcludingExtendedElements();
            Assert.IsFalse(crit.IsIncludeExtendedElements);

            crit.IncludingExtendedElements();
            Assert.IsTrue(crit.IsIncludeExtendedElements);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void IncludeExtendedElements()
        {
            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .SetIncludeExtendedElements(true);

            Assert.IsTrue(crit.IsIncludeExtendedElements);

            crit.SetIncludeExtendedElements(false);
            Assert.IsFalse(crit.IsIncludeExtendedElements);

            crit.SetIncludeExtendedElements(true);
            Assert.IsTrue(crit.IsIncludeExtendedElements);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void DescendingFrom()
        {
            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .DescendingFrom("A/Path/Here");

            Assert.AreEqual("A/Path/Here", crit.AncestryPath);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void Reset_With_Options_ChangeNothing()
        {
            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .WithBalanceTypes(new Element.BalanceType[] {Element.BalanceType.credit, Element.BalanceType.debit, Element.BalanceType.na})
                .WithOptionalWords("Blah blah")
                .WithRequiredWords("Blab blab")
                .DescendingFrom("Test/Path/Here");

            crit.Reset(LibrarySearchCriteriaResetOptions.ChangeNothing);

            Assert.AreEqual(new Element.BalanceType[] { Element.BalanceType.credit, Element.BalanceType.debit, Element.BalanceType.na },crit.BalanceTypes);
            Assert.AreEqual(new string[]{"Blah","blah"},crit.OptionalWords);
            Assert.AreEqual(new string[] { "Blab", "blab" }, crit.RequiredWords);
            Assert.AreEqual("Test/Path/Here", crit.AncestryPath);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void Reset_With_Options_Full()
        {
            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .WithBalanceTypes(new Element.BalanceType[] { Element.BalanceType.credit, Element.BalanceType.debit, Element.BalanceType.na })
                .WithOptionalWords("Blah blah")
                .WithRequiredWords("Blab blab")
                .DescendingFrom("Test/Path/Here");

            crit.Reset(LibrarySearchCriteriaResetOptions.Full);

            AssertFullResetState(crit);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void Reset_With_Options_KeepPath()
        {
            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .WithBalanceTypes(new Element.BalanceType[] { Element.BalanceType.credit, Element.BalanceType.debit, Element.BalanceType.na })
                .WithOptionalWords("Blah blah")
                .WithRequiredWords("Blab blab")
                .DescendingFrom("Test/Path/Here");

            crit.Reset(LibrarySearchCriteriaResetOptions.Full.WithKeepPath());

            Assert.AreEqual(new Element.BalanceType[]{}, crit.BalanceTypes);
            Assert.AreEqual(new string[]{}, crit.OptionalWords);
            Assert.AreEqual(new string[]{}, crit.RequiredWords);
            Assert.AreEqual("Test/Path/Here", crit.AncestryPath);
        }
    }
}
#endif
