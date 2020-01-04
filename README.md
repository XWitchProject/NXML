# NXML

NXML is a handwritten two-way parser and scripted transformation engine for XML\*, written in C#. It allows you to read and write XML\* files. The XML.Script subproject provides a runtime and an API for Lua scripts to transform any XML\* files in a fairly straightforward manner.

## \*Not Actually XML

NXML was created specifically for Noita. Noita makes use of a custom parser that pretends to be an XML parser but actually violates about half of the standard, causing endless headaches for people who want to create external tools. Its mere existence is a violation of the XML specification, as it's a highly lenient parser - it feels more like a weird mix of HTML and XHTML. Even the files the actual game uses reflect this pretty well - full of typos, mistakes and other trash.

As an example, the [Ommel](https://github.com/NoitaOmmel/Ommel) project currently uses a [modified version of an HTML parsing library](https://github.com/NoitaOmmel/HTMLAgilityPack) to even be able to *read* these files - as expected, most XML parsing libraries follow the spec quite closely and are very strict, as they very well should. NXML provides you a library to work with these "noita flavored XHTML/HTML" files. With NXML you can ensure that not only will the data you read be correct, the data you write will also be as the game expects it to be. An example here is that Noita's parser lacks any special handling for character entities like `&lt;` - however, it will happily accept literal characters that are otherwise illegal like `<` in attribute values. This is completely opposite to what the spec says and what every single XML library out there implements, but here we are.

## Parsing

NXML is based on code from [poro](https://github.com/gummikana/poro/blob/SDL2/source/utils/xml/cxmlparser.cpp) - this is the library that Noita actually uses for, among other things, parsing XML files and it just so happens to be written by most of the people who actually work on Noita. It's licensed under the FSF&OSI-approved zlib license, therefore there should be no licensing issues. NXML parses XML files exactly the same as Noita.

# NXML.Script

NXML.Script runs on top of the standard Lua 5.1 implementation (or LuaJIT) to expose an API for XML element manipulation/transformation. NXML.Script enables you to use a Lua script to modify an XML file procedurally, which gives you a lot of freedom to implement automated tasks easily.

## Example

```lua
function Transform(elem)
        elem.Children:iter(function(i, el)
                Transform(el)
        end)

        if elem.Name == "Entity" then
                local new_luacomp = xmlnew("LuaComponent")
                local a = new_luacomp.Attributes
                a.script_source_file = "mods/blah/etc/test.lua"
                a.vm_type = "ONE_PER_COMPONENT_INSTANCE"
                a.execute_on_added = "1"
                a.execute_every_n_frame = "-1"
                a.execute_times = "1"
                a.enable_coroutines = "1"

                elem.Children:insert(1, new_luacomp)
        end
end
```

Before:
```xml
<Entity>
    <ElectricityComponent energy="0" probability_to_heat="0" speed="0" />
    <LuaComponent blah="blah" />
</Entity>
```

After:
```xml
<Entity>
    <LuaComponent script_source_file="mods/blah/etc/test.lua" vm_type="ONE_PER_COMPONENT_INSTANCE" execute_on_added="1" execute_every_n_frame="-1" execute_times="1" enable_coroutines="1"/>
    <ElectricityComponent energy="0" probability_to_heat="0" speed="0"/>
    <LuaComponent blah="blah"/>
</Entity>
```
