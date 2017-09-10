# English

Download all your complete order history from amazon.de (!) using your browser's authorization cookies.

# German

Alle Bestellungen und alle Produkte, die Sie jemals bei amazon.de gekauft haben herunterladen und exportieren. Benutzt Autorisierungscookies Ihres Browsers.

# Anleitung

Loggen Sie sich mit [Mozilla Firefox](https://www.mozilla.org/de/firefox) oder [Google Chrome](https://www.google.com/chrome/browser/desktop/index.html) bei Amazon.de ein, öffnen Sie eine Konsole im Verzeichnis, in dem Sie den Amazon-Scraper entpackt haben. Laden Sie zunächst Ihre Bestellungen herunter, exportieren Sie sie im CSV-Format, um sie bspw. mit LibreOffice weiter zu verarbeiten, leeren Sie dann optional den lokalen Cache:

```
PS> amz-scrape
PS> amz-dump > bestellungen.txt
PS> amz-scrape -clean
```

# License

The MIT License (MIT)

Copyright (c) 2015 Christoph Stoepel (http://christoph.stoepel.net)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
