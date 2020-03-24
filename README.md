# JarBindingArgumentNamesParse
Simple parser that parses .aidl files and generates metadata.xml managedName attributes for method arguments.


From this:

```java
package com.comp.product.aidl.interop;

interface AidlInterface {
	boolean loadKey(byte type,in byte[] keyVal,byte tmkIndex);
}
```
To this:

```XML
<attr path="/api/package[@name='com.comp.product.aidl.interop']/interface[@name='AidlInterface']/method[@name='loadKey']/parameter[@name='p0']" name="managedName">type</attr>
<attr path="/api/package[@name='com.comp.product.aidl.interop']/interface[@name='AidlInterface']/method[@name='loadKey']/parameter[@name='p1']" name="managedName">keyVal</attr>
<attr path="/api/package[@name='com.comp.product.aidl.interop']/interface[@name='AidlInterface']/method[@name='loadKey']/parameter[@name='p2']" name="managedName">tmkIndex</attr>
```
