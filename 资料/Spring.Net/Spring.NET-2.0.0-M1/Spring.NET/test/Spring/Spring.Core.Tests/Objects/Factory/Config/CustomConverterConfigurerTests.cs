#region License

/*
 * Copyright � 2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

#region Imports

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;

#endregion

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Unit tests for the CustomConverterConfigurer class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class CustomConverterConfigurerTests
    {
	    private MockRepository mocks;
	    private IConfigurableListableObjectFactory factory;

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            factory = (IConfigurableListableObjectFactory) 
                mocks.CreateMock(typeof(IConfigurableListableObjectFactory));

        }

		[Test]
		[ExpectedException(typeof(ObjectInitializationException))]
        public void UseInvalidKeyForConverterMapKey()
		{
        	IDictionary converters = new Hashtable();
			converters.Add(12, typeof(DateTimeConverter));

            CustomConverterConfigurer config = new CustomConverterConfigurer();
			config.CustomConverters = converters;
			config.PostProcessObjectFactory(factory);

            mocks.VerifyAll();
		}

		[Test]
		[ExpectedException(typeof(ObjectInitializationException))]
		public void UseNonTypeConverterForConverterMapValue()
		{
			IDictionary converters = new Hashtable();
			converters.Add( typeof(DateTime), null);

			CustomConverterConfigurer config = new CustomConverterConfigurer();
			config.CustomConverters = converters;
			config.PostProcessObjectFactory(factory);
        
            mocks.VerifyAll();
        }

		[Test]
		[ExpectedException(typeof(ObjectInitializationException))]
		public void UseNonResolvableTypeForConverterMapKey()
		{
			IDictionary converters = new Hashtable();
			// purposely misspelled... :D
			converters.Add("Systemm.Date", typeof(DateTimeConverter));

			CustomConverterConfigurer config = new CustomConverterConfigurer();
			config.CustomConverters = converters;
			config.PostProcessObjectFactory(factory);
        
            mocks.VerifyAll();
        }

		/// <summary>
		/// Just tests that the configurer doesn't blow up and
		/// doesn't register anything ('cos we ain't supplied anything).
		/// </summary>
		[Test]
		public void DontSupplyAnyCustomConverters()
		{
			CustomConverterConfigurer config = new CustomConverterConfigurer();
			config.CustomConverters = null;
			config.PostProcessObjectFactory(factory);
			mocks.ReplayAll();

            mocks.VerifyAll();
		}

		[Test]
		public void SunnyDayScenario()
		{
			IDictionary converters = new Hashtable();
		    Type dateTimeType = typeof(DateTime);
            DateTimeConverter dateTimeConverter = new DateTimeConverter();
            Type colorType = typeof(Color);
            ColorConverter colorConverter = new ColorConverter();
		    
            converters.Add(dateTimeType, dateTimeConverter);
		    converters.Add(colorType, colorConverter);

            factory.RegisterCustomConverter(dateTimeType, dateTimeConverter);
            factory.RegisterCustomConverter(colorType, colorConverter);

            mocks.ReplayAll();

			CustomConverterConfigurer config = new CustomConverterConfigurer();
			config.CustomConverters = converters;
			config.PostProcessObjectFactory(factory);

            mocks.VerifyAll();

		}
    }
}
