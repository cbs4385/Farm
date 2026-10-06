using Farm.Core;
using Farm.Data;
using UnityEngine;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Parcels (items that did not fit the backpack wait in the mailbox), villager-kept seats, and the library desk's extra books.
    public class ParcelsAndSeatsTests
    {
        [Test]
        public void ParcelVar_IsPrefixedWithTheItemId() => Assert.AreEqual("parcel.prop.book", Parcels.VarFor("prop.book"));

        [Test]
        public void Seat_RestsOncePerDay()
        {
            Assert.IsTrue(SeatSpot.CanRest(0, 0), "never used (var 0)");
            Assert.IsFalse(SeatSpot.CanRest(5, 4), "used on day 4 (stored +1)");
            Assert.IsTrue(SeatSpot.CanRest(5, 5), "used yesterday");
        }

        [Test]
        public void LibraryDesk_BookFlagsAreUniqueAndTheFirstKeepsItsOldFlag()
        {
            Assert.AreEqual(LibraryDesk.TakenFlag, LibraryDesk.Books[0].Flag);
            var flags = new System.Collections.Generic.HashSet<string>();
            foreach (var b in LibraryDesk.Books) Assert.IsTrue(flags.Add(b.Flag), b.Id);
        }

        [TestCase("carving")]
        [TestCase("charm")]
        [TestCase("feather")]
        [TestCase("hat")]
        [TestCase("sock")]
        [TestCase("ribbon")]
        [TestCase("anvil")]
        [TestCase("clasp")]
        [TestCase("horseshoe")]
        [TestCase("pinecone")]
        [TestCase("drawing")]
        [TestCase("salve")]
        [TestCase("scarf")]
        [TestCase("longbook")]
        public void StoryProps_AreItemsWithNames(string prop)
        {
            var db = Resources.Load<GameDatabase>("GameDatabase");
            Assert.IsTrue(db.TryGetItem("prop." + prop, out _), prop);
            Assert.AreNotEqual("item.prop." + prop + ".name", L.Get("item.prop." + prop + ".name"));
        }

        [TestCase("hover.curio")]
        [TestCase("curio.catdoor")]
        [TestCase("seat.nook.friend")]
        [TestCase("seat.couch.plain")]
        [TestCase("seat.rock.friend")]
        [TestCase("hover.seat")]
        [TestCase("seat.rested")]
        [TestCase("seat.window.friend")]
        [TestCase("seat.window.plain")]
        [TestCase("seat.stool.friend")]
        [TestCase("seat.stool.plain")]
        [TestCase("seat.chair.friend")]
        [TestCase("seat.chair.plain")]
        [TestCase("mailbox.parcel")]
        [TestCase("mailbox.parcel_full")]
        [TestCase("toast.parcel_waiting")]
        [TestCase("item.prop.almanac.name")]
        [TestCase("item.prop.fieldguide.name")]
        public void NewStrings_Exist(string key) => Assert.AreNotEqual(key, L.Get(key));
    }
}
