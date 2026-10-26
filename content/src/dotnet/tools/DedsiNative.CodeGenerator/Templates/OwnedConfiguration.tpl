        {{Builder}}.OwnsOne(x => x.{{Name}}, {{OwnedBuilder}} =>
        {
{{Fields}}
        });
        {{Builder}}.Navigation(x => x.{{Name}}).IsRequired({{Required}});
