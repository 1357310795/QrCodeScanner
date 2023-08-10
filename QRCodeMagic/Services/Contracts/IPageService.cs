using System;

namespace QRCodeMagic.Services.Contracts;

public interface IPageService
{
    Type GetPageType(string key);
}
