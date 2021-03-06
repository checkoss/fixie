﻿namespace Fixie.Internal
{
    using System.Threading.Tasks;

    public interface Handler<in TMessage> : Listener where TMessage : Message
    {
        void Handle(TMessage message);
    }

    public interface AsyncHandler<in TMessage> : Listener where TMessage : Message
    {
        Task Handle(TMessage message);
    }
}