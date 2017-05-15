# PdfSign
Tool to add invisible e-signature to pdf document with iTextSharp library

## Usage
* Import your certificate into windows certstore as **exportable**.
* Update app.config
* Sign pdf: `pdfsign.exe mydoc.pfd`
* NOTE: can be used in batch (see result codes)

## Configuration
Configuration is stored in app.config 
* `certSubjectName` - initial and unique part of certificate subject
* `reason` - will be written to the pdf file
* `location` - will be written to the pdf file
* `allowInvalidCertificate` - true/false. Allows you to use invalid certificate in test environment
* `backupOriginalFile` - true/false. If allowed, original (not signed) file copy is backuped as *.original

## Result codes
* 0 - success
* 1 - invalid parameters
* 2 - pdf file not found
* 4 - other error (usualy certificate not found)

---
*NOTE: This tool is just publication of my older work - so it uses a little bit older version of iTextSharp.*



