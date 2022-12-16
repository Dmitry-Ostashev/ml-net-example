using System;
using DevExpress.Xpo;

namespace MlNetExample {
    public class ModelStorage : XPObject {
        public ModelStorage(Session session) : base(session) {}

        private string name;
        public string Name {
            get => name;
            set => SetPropertyValue<string>(nameof(Name), ref name, value);
        }

        [Size(SizeAttribute.Unlimited)]
        public byte[] Content {
            get => GetPropertyValue<byte[]>(nameof(Content));
            set => SetPropertyValue<byte[]>(nameof(Content), value);
        }
    }
}