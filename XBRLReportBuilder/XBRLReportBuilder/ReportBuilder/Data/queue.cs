//--------------------------------------------------------------
// X_Filing (Class)
// Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
// This data class is used to impelemnt the processing queue.
//------------------------------------------------------------------------------

using System.Xml.Serialization;


/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/queue.xsd")]
[System.Xml.Serialization.XmlRootAttribute(Namespace="http://tempuri.org/queue.xsd", IsNullable=false)]
public class X_Filings {
    
    /// <remarks/>
    public string downloadDate;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("X_Filing")]
    public X_FilingsX_Filing[] X_Filing;
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/queue.xsd")]
public class X_FilingsX_Filing {
    
    /// <remarks/>
    public string cik_number;
    
    /// <remarks/>
    public string accession_number;
    
    /// <remarks/>
    public string company_name;
    
    /// <remarks/>
    public string form_type;
    
    /// <remarks/>
    public string filing_date;
    
    /// <remarks/>
    public string file_number;
    
    /// <remarks/>
    public string period_ending;
    
    /// <remarks/>
    public string sec_act;
    
    /// <remarks/>
    public string sic_code;
    
    /// <remarks/>
    public string sic_name;
    
    /// <remarks/>
    public string irs_number;
    
    /// <remarks/>
    public string state_of_inc;
    
    /// <remarks/>
    public string fiscal_year_end;
    
    /// <remarks/>
    public string ticker_symbol;
    
    /// <remarks/>
    public string bus_addr1;
    
    /// <remarks/>
    public string bus_addr2;
    
    /// <remarks/>
    public string bus_addr3;
    
    /// <remarks/>
    public string bus_city;
    
    /// <remarks/>
    public string bus_state;
    
    /// <remarks/>
    public string bus_zip;
    
    /// <remarks/>
    public string bus_phone;
    
    /// <remarks/>
    public string mail_addr1;
    
    /// <remarks/>
    public string mail_addr2;
    
    /// <remarks/>
    public string mail_addr3;
    
    /// <remarks/>
    public string mail_city;
    
    /// <remarks/>
    public string mail_state;
    
    /// <remarks/>
    public string mail_zip;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("X_File")]
    public X_FilingsX_FilingX_File[] X_File;
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/queue.xsd")]
public class X_FilingsX_FilingX_File {
    
    /// <remarks/>
    public string sequence;
    
    /// <remarks/>
    public string file;
    
    /// <remarks/>
    public string name;
    
    /// <remarks/>
    public string type;
    
    /// <remarks/>
    public string size;
    
    /// <remarks/>
    public string description;
    
    /// <remarks/>
    public string url;
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/queue.xsd")]
[System.Xml.Serialization.XmlRootAttribute(Namespace="http://tempuri.org/queue.xsd", IsNullable=false)]
public class NewDataSet {
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("X_Filings")]
    public X_Filings[] Items;
}
