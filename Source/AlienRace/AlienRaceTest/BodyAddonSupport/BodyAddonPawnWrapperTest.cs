namespace AlienRaceTest.BodyAddonSupport
{
    using System.Runtime.Serialization;
    using AlienRace.BodyAddonSupport;
    using Moq;
    using NUnit.Framework;
    using TestSupport;
    using Verse;

    [TestFixture]
    public class BodyAddonPawnWrapperTest : BaseUnityTest
    {
        private Pawn                 pawn;
        private BodyAddonPawnWrapper pawnWrapperUnderTest;

        [SetUp]
        public void SetupBodyAddonPawnWrapperTest()
        {
            this.pawn = new Pawn();
            Pawn_HealthTracker healthTracker =
                (Pawn_HealthTracker)FormatterServices
                .GetUninitializedObject(typeof(Pawn_HealthTracker)); //does not call ctor
            healthTracker.hediffSet   = new HediffSet(this.pawn);
            this.pawn.health          = healthTracker;
            this.pawnWrapperUnderTest = new BodyAddonPawnWrapper(this.pawn);
        }

        [Test]
        public void TestHediffsOnPartAreAlwaysFalseForFullBodyHediffs()
        {
            Mock<Hediff> fullBodyHediff = new Mock<Hediff>();
            this.pawn.health.hediffSet.hediffs.Add(fullBodyHediff.Object);

            Assert.IsFalse(this.pawnWrapperUnderTest.IsPartBelowHealthThreshold("leg", 0f));
            Assert.IsFalse(this.pawnWrapperUnderTest.IsPartBelowHealthThreshold("leg", 1f));
            Assert.IsFalse(this.pawnWrapperUnderTest.IsPartBelowHealthThreshold("",    1f));
            Assert.IsFalse(this.pawnWrapperUnderTest.IsPartBelowHealthThreshold(null,  1f));
        }
    }
}