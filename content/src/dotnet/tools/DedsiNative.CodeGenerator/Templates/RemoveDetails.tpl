{{Indent}}foreach (var {{Variable}} in {{Owner}}.{{Field}})
{{Indent}}{
{{Children}}{{Indent}}    dbContext.Remove({{Variable}});
{{Indent}}}
