using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoNSubstitute;
using Ploeh.AutoFixture.Xunit2;

namespace UnitTests
{
    public class AutoSubstituteDataAttribute : AutoDataAttribute
    {
        public AutoSubstituteDataAttribute() : 
            base(new Fixture().Customize(new AutoConfiguredNSubstituteCustomization()))
        {
            
        }
        
    }
}