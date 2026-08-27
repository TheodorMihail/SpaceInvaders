using NSubstitute;
using NUnit.Framework;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using UnityEngine;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class PlayfieldServiceTests
    {
        private const float FloatTolerance = 0.0001f;

        private const float ReferenceOrthographicSize = 150f;
        private const float ReferenceAspectRatio = 16f / 9f;

        private const float NarrowAspectRatio = 4f / 3f;
        private const float WideAspectRatio = 21f / 9f;
        private const float UltraWideAspectRatio = 32f / 9f;

        private static float GetPlayfieldSize(float screenAspectRatio)
        {
            return PlayfieldService.GetPlayfieldOrthographicSize(ReferenceOrthographicSize, ReferenceAspectRatio, screenAspectRatio);
        }

        private static Rect GetRect(float screenAspectRatio)
        {
            float playfieldSize = GetPlayfieldSize(screenAspectRatio);

            return PlayfieldService.GetViewportRect(ReferenceOrthographicSize, ReferenceAspectRatio, screenAspectRatio, playfieldSize);
        }

        [Test]
        public void GetPlayfieldOrthographicSize_AtReferenceAspect_KeepsAuthoredSize()
        {
            Assert.AreEqual(ReferenceOrthographicSize, GetPlayfieldSize(ReferenceAspectRatio), FloatTolerance);
        }

        [Test]
        public void GetPlayfieldOrthographicSize_OnWiderAspect_KeepsAuthoredSize()
        {
            Assert.AreEqual(ReferenceOrthographicSize, GetPlayfieldSize(WideAspectRatio), FloatTolerance);
        }

        [Test]
        public void GetPlayfieldOrthographicSize_OnNarrowerAspect_ZoomsOutToFitTheWidth()
        {
            Assert.AreEqual(200f, GetPlayfieldSize(NarrowAspectRatio), FloatTolerance);
        }

        [Test]
        public void GetPlayfieldOrthographicSize_WithNoAspect_KeepsAuthoredSize()
        {
            Assert.AreEqual(ReferenceOrthographicSize, GetPlayfieldSize(0f), FloatTolerance);
        }

        [Test]
        public void GetViewportRect_AtReferenceAspect_FillsTheView()
        {
            Rect rect = GetRect(ReferenceAspectRatio);

            Assert.AreEqual(0f, rect.x, FloatTolerance);
            Assert.AreEqual(0f, rect.y, FloatTolerance);
            Assert.AreEqual(1f, rect.width, FloatTolerance);
            Assert.AreEqual(1f, rect.height, FloatTolerance);
        }

        [Test]
        public void GetViewportRect_OnWiderAspect_LeavesBandsAtTheSides()
        {
            Rect rect = GetRect(WideAspectRatio);

            Assert.AreEqual(ReferenceAspectRatio / WideAspectRatio, rect.width, FloatTolerance);
            Assert.AreEqual(1f, rect.height, FloatTolerance);
            Assert.AreEqual((1f - rect.width) * 0.5f, rect.x, FloatTolerance);
        }

        [Test]
        public void GetViewportRect_OnNarrowerAspect_LeavesBandsAboveAndBelow()
        {
            Rect rect = GetRect(NarrowAspectRatio);

            Assert.AreEqual(1f, rect.width, FloatTolerance);
            Assert.AreEqual(0.75f, rect.height, FloatTolerance);
            Assert.AreEqual(0.125f, rect.y, FloatTolerance);
        }

        [Test]
        public void GetViewportRect_OnUltraWideAspect_HalvesTheWidth()
        {
            Rect rect = GetRect(UltraWideAspectRatio);

            Assert.AreEqual(0.5f, rect.width, FloatTolerance);
            Assert.AreEqual(0.25f, rect.x, FloatTolerance);
        }

        /// <summary>The whole point of the setup: the same world units on every display.</summary>
        [Test]
        public void GetViewportRect_OnAnyAspect_CoversTheAuthoredWorldSize()
        {
            float[] aspectRatios = { NarrowAspectRatio, 1.6f, ReferenceAspectRatio, WideAspectRatio, UltraWideAspectRatio };

            foreach (float aspectRatio in aspectRatios)
            {
                float playfieldSize = GetPlayfieldSize(aspectRatio);
                Rect rect = GetRect(aspectRatio);

                float worldHalfWidth = rect.width * playfieldSize * aspectRatio;
                float worldHalfHeight = rect.height * playfieldSize;

                Assert.AreEqual(ReferenceOrthographicSize * ReferenceAspectRatio, worldHalfWidth, FloatTolerance * 100f);
                Assert.AreEqual(ReferenceOrthographicSize, worldHalfHeight, FloatTolerance * 100f);
            }
        }

        [Test]
        public void GetViewportRect_OnAnyAspect_StaysCentred()
        {
            Rect rect = GetRect(WideAspectRatio);

            Assert.AreEqual(0.5f, rect.center.x, FloatTolerance);
            Assert.AreEqual(0.5f, rect.center.y, FloatTolerance);
        }

        [Test]
        public void ToViewportPoint_BeforeSetup_MapsStraightThrough()
        {
            IPlayfieldService playfield = CreateService();

            Assert.AreEqual(new Vector2(0.25f, 0.75f), playfield.ToViewportPoint(0.25f, 0.75f));
        }

        [Test]
        public void ViewportRect_BeforeSetup_FillsTheView()
        {
            IPlayfieldService playfield = CreateService();

            Assert.AreEqual(new Rect(0f, 0f, 1f, 1f), playfield.ViewportRect);
        }

        [Test]
        public void SetupToPlayfield_WithNoCamera_LeavesTheAreaAlone()
        {
            IPlayfieldService playfield = CreateService();
            playfield.SetupToPlayfield(null);

            Assert.AreEqual(new Rect(0f, 0f, 1f, 1f), playfield.ViewportRect);
        }

        private static IPlayfieldService CreateService()
        {
            var gameDataConfig = ScriptableObject.CreateInstance<GameDataConfigSO>();
            var gameRepository = Substitute.For<IGameRepository>();
            gameRepository.GetGameDataConfig().Returns(gameDataConfig);

            return new PlayfieldService(gameRepository);
        }
    }
}
