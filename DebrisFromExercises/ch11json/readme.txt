ch11json
=========

Changes from the previous trees (ch11trees):
  
    1.  There are a couple of important, extra fields for data that is
        needed from the Symbol Table but wasn't in the original trees:
        
          - subroutineDecs include 'VarCount', the number of local 
            variables in the subroutine.
            
          - For a constructor the subroutineDec also includes the 
            'FieldCount' for the class it's constructing.
        
    2.  There's now a tree in the JSON format.
    
    3.  There are samples of how to consume JSON using C#, Python and C++.
          
          - C# uses Json.Net from Newtonsoft.  It can be downloaded
            using Nuget (or from http://james.newtonking.com/json)
            
          - Python uses the bult-in JSON support.
          
          - C++ uses the 'jsoncpp' library available on github:
            https://github.com/open-source-parsers/jsoncpp
         
    4.  Removed the 'category' and 'beingDefined' properties.
    
    5.  Updated the CodeWriter to remove dependency on the Symbol
        Table and 'category' (to make sure there wasn't any other 
        missing info!).
            
