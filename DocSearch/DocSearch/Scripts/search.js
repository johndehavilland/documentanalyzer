var lastResults;
var graphVisible = false;

String.prototype.replaceAll = function (search, replacement) {
    var target = this;
    return target.replace(new RegExp(search, 'g'), replacement);
};

Array.prototype.flatMap = function (lambda) {
    return Array.prototype.concat.apply([], this.map(lambda));
};


function toggleGraph() {
    setGraphVisible(!graphVisible)
}

function setGraphVisible(visible) {
    if (!graphVisible && automagic.store.getState().results.count > 0
        && automagic.store.getState().parameters.input != graphQuery)

        getFDNodes(automagic.store.getState().parameters.input);

    graphVisible = visible;
    if (graphVisible)
        $("#fdGraph").show();
    else
        $("#fdGraph").hide()
}

function highlight(orig, text) {
    text = escapeRegExp(text);
    orig = orig.replaceAll(text, '<span class="highlight" style="background-color: #FFFF00">' + text + '</span>');
    return orig;
}

function escapeRegExp(str) {
    return str.replace(/[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g, "\\$&");
}


function showDocument(index) {
    var regex = /(<([^>]+)>)/ig;

    result = lastResults[index];

    var highlights = $.unique(
        result['@search.highlights'].text
            .flatMap(function (t) { return t.replace(regex, "") })
            .map(function (t) { return t; })
    );


    for (var h in highlights) {

        var high = highlights[h]
        if (typeof (high) !== 'string') {
            continue;
        }
        result.text = highlight(result.text, high);
    }

    result.text = result.text.replace(/\n/g, "<br />");

    hocrProofreader.currentPage = null;
    if (result.metadata === "empty") {
        result.metadata = result.text;
    }
    hocrProofreader.setHocr(result.metadata, "", result.pageNum, result.keywords);
    //hocrProofreader.setHocr(result.text, "", "first", result.entities);
    hocrProofreader.setZoom('page-full');

    $('#download-original').click(function () {
        window.location = "/Home/DownloadBlob/?name=" + result.file;
    });

    $('#myModal').modal('show');

    $('#myModal').one('shown.bs.modal', function () {
        //document.getElementsByTagName('h2')[3].scrollIntoView();
        hocrProofreader.editorIframe.contentDocument.body.getElementsByClassName('highlight')[0].scrollIntoView({ behavior: "instant", block: "start" });
        //hocrProofreader.editorIframe.contentDocument.body.children[result.pageNum].scrollIntoView({ behavior: "instant", block: "start" });
    });

}

function getHocrMetadata(element) {
    var meta = {};
    meta.text = element.innerText;
    $(element).attr("title").split(";").map(function (e) { return e.trim(); }).forEach(function (e) {
        var sections = e.split(" ")
        var vals = [];
        for (var i = 1; i < sections.length; i++) {
            if (sections[i].substring(0, 1) == '"' || sections[i].substring(0, 1) == "'")
                vals.push(sections[i].substring(1, sections[i].length - 1));
            else
                vals.push(Number(sections[i]));
        }

        meta[sections[0]] = vals.length == 1 ? vals[0] : vals;
    });
    return meta;
}

//hack
var imageAspectRatio;
var aRatio;
var firstHighlight;
var viewOffset;


function OnSearch(searchState, postBody) {
    notifywiki = true;
    for (var facet = 0; facet < postBody.facets.length; facet++) {
        postBody.facets[facet] = postBody.facets[facet].replace("tags,count:5,", "tags,count:20,");
    }

    //if (graphVisible)
    getFDNodes(postBody.search);

    return fetch("/api/search",
        {
            mode: "cors",
            headers: {
                "api-key": searchState.config.queryKey,
                "Content-Type": "application/json"
            },
            method: "POST",
            body: JSON.stringify(postBody)
        });
}


// Initialize and connect to your search service
var automagic = new AzSearch.Automagic({});
// add a results view using the template defined above
automagic.addResults("results", { count: true });
// Adds a pager control << 1 2 3 ... >>
automagic.addPager("pager");

automagic.addSearchBox("searchBox");

automagic.addCheckboxFacet("entitiesFacet", "entities", "collection");

// get hightlight snippets for text
automagic.store.updateSearchParameters({ highlight: "text", top: 10, searchMode: "all" });

//$("#facetPanel").change(OnSearch);
automagic.store.setSearchCallback(OnSearch);

var resultTemplate =
    `<div>{{{id}}}: {{{title}}}</div>
    {{#metadata}}
       <div style="border: #286090 solid 2px;">
       <div class="stretchy-wrapper" style="padding-bottom: {{{image.previewAspectRatio}}}%;{{{extraStyles}}}">
           <div class ="resultDescription" style="border: solid; border-width: 0px; background-image:url('{{{image.url}}}'); background-size:100%; background-position-y:{{{image.topPercent}}}%; overflow: hidden;" onclick="showDocument({{{idx}}})">
             {{#highlight_words}}

                <div class ="highlight" style="position: absolute; width: {{{widthPercent}}}%; height: {{{heightPercent}}}%; left: {{{leftPercent}}}%; top: {{{topPercent}}}%;" title="{{annotation}}" data-wikipedia="{{wikipediaUrl}}"></div>

             {{/highlight_words}}
           </div>
       </div>
       </div>
    {{/metadata}}`;

// add a results view, updates parameters to include count, uses the above template
automagic.addResults("results", { count: true }, resultTemplate);

automagic.store.setInput("");

var notifywiki = false;

// add a resultsProcessor to more easily format the results display
var resultsProcessor = function (results) {
    var idx = 0;
    return lastResults = results.map(function (result) {

        if (notifywiki) {
            notifywiki = false;
            window.setTimeout(function () { setupAnnotations(); }, 100);
        }
        result.idx = idx;
        idx++;
        var viewAspectRatio = .2;

        // get all the unique highlight words
        var highlights = $.unique(
            result['@search.highlights'].text
                .flatMap(function (t) { return t.match(/<em>(.+?)<\/em>/g) })
                .map(function (t) { return t.substring(4, t.length - 5).toLowerCase(); })
        );
        result.keywords = highlights;


        if (!result.metadata) {
            result.metadata = "empty";
            result.image = {
                url: "/Content/pdf_logo.png",
                previewAspectRatio: viewAspectRatio * 100,
                topPercent: 10,
                width: 100,
                height: 100
            };
            
            return result;
        }

        result.extraStyles = "";
        

        if (!result.metadata) 
            return result;


        var pages = $(result.metadata).filter(".ocr_page");
        if (pages.length == 0)
            return result;


        var noHighlights = function (pagenum) {
            var pageMeta = getHocrMetadata(pages[pagenum | 0]);
            result.image = {
                url: pageMeta.image,
                width: pageMeta.bbox[2],
                height: pageMeta.bbox[3],
                previewAspectRatio: viewAspectRatio * 100,
                topPercent: 10,
            };
            result.pageNum = pageMeta.ppageno;
            return result;
        }

        if (!result['@search.highlights'])
            return noHighlights();

        // get all the highlighted word metadata
        var previewPages = pages.map(function (i, page) {
            return {
                metadata: getHocrMetadata(page),
                highlightNodes: $("span[class='ocrx_word']", page).filter(function () { return highlights.includes($(this).text().toLowerCase()); }).toArray()
            };
        }).toArray();

        var pageIdx = previewPages.findIndex(function (page) { return page.highlightNodes.length > 0; });
        if (pageIdx == -1)
            pageIdx = 0;
        var previewPage = previewPages[pageIdx];

        // override the preview page if set on the index
        if (result.demoInitialPage && previewPages[result.demoInitialPage].highlightNodes.length > 0)
            previewPage = previewPages[result.demoInitialPage];

        if (previewPage.highlightNodes.length == 0)
            return noHighlights(result.demoIntialPage);

        // determine the page to show
        var pageMeta = previewPage.metadata;
        result.pageNum = pageMeta.ppageno;
        result.image = {
            url: pageMeta.image,
            width: pageMeta.bbox[2],
            height: pageMeta.bbox[3],
            previewAspectRatio: viewAspectRatio * 100
        };

        // get the highlighted words
        highlightsToShow = previewPage.highlightNodes
            .map(function (node) { return getHocrMetadata(node); })
            .filter(function (meta) { return meta.bbox; })
            .map(function (keywordMeta) {
                return {
                    left: keywordMeta.bbox[0],
                    top: keywordMeta.bbox[1],
                    width: keywordMeta.bbox[2] - keywordMeta.bbox[0],
                    height: keywordMeta.bbox[3] - keywordMeta.bbox[1],
                    text: keywordMeta.text
                };
            });

        result.highlightsToShow = highlightsToShow;
        if (highlightsToShow.length > 0) {
                // determine the image aspect ratio
                /*var*/ imageAspectRatio = result.image.height / result.image.width;
                /*var*/ aRatio = viewAspectRatio / imageAspectRatio;

                // show 2 lines above the highlighted word, but make sure not to offset past top of the document
                /*var*/ firstHighlight = highlightsToShow[0];
                /*var*/ viewOffset = firstHighlight.height * 2;
            if (firstHighlight.top - viewOffset < 0)
                viewOffset = -firstHighlight.top;

            viewh = (aRatio * result.image.height);

            result.image.topPercent = (firstHighlight.top - viewOffset) / (result.image.height - viewh) * 100;

            viewt = (result.image.height - viewh) * (result.image.topPercent / 100);

            result.highlight_words = highlightsToShow.map(function (word) {
                var highlight_word = {
                    heightPercent: word.height / result.image.height / aRatio * 100,
                    //topPercent: viewOffset / (result.image.height - (word.height * aRatio)) / aRatio * 100,
                    topPercent: ((word.top - viewt) * (100 / viewh)),
                    widthPercent: word.width / result.image.width * 100,
                    leftPercent: word.left / result.image.width * 100
                }

                return highlight_word;
            });
        }
        else {
            // a photo
            result.image.topPercent = 50;
            result.image.previewAspectRatio = 40;
            result.extraStyles = "; max-width: 600px; margin-left: auto; margin-right: auto";
        }


        return result;
    });
};
automagic.store.setResultsProcessor(resultsProcessor);

function setupAnnotations() {
    $('div.highlight[data-wikipedia!=""]').tooltipster({
        content: 'Loading data from Wikipedia...',
        theme: 'tooltipster-shadow',
        updateAnimation: null,
        interactive: true,
        maxWidth: 600,
        // 'instance' is basically the tooltip. More details in the "Object-oriented Tooltipster" section.
        functionBefore: function (instance, helper) {

            var $origin = $(helper.origin);

            // we set a variable so the data is only loaded once via Ajax, not every time the tooltip opens
            if ($origin.data('loaded') !== true) {
                var page_id = $origin.attr("data-wikipedia");
                fetchWiki(page_id, function (data) {

                    // call the 'content' method to update the content of our tooltip with the returned data.
                    // note: this content update will trigger an update animation (see the updateAnimation option)
                    data.find("sup.reference").remove();
                    data.find("a").attr({ href: "" }).click(function (elem) {
                        automagic.store.setInput(elem.toElement.innerText);
                        automagic.store.search();
                        return false;
                    });
                    instance.content(data);

                    // to remember that the data has been loaded
                    $origin.data('loaded', true);
                });
            }
        }
    });

    $('div.highlight[title!=""]').tooltipster({
        theme: 'tooltipster-shadow',
        maxWidth: 400,
    });
}


function fetchWiki(page_id, callback) {
    $.ajax({
        url: "https://en.wikipedia.org/w/api.php",
        data: {
            format: "json",
            action: "parse",
            page: page_id,
            prop: "text",
            section: 0,
            origin: "*",
        },
        dataType: 'json',
        headers: {
            'Api-User-Agent': 'MyCoolTool/1.1 (http://example.com/MyCoolTool/; MyCoolTool@example.com) BasedOnSuperLib/1.4'
        },
        crossDomain: true,
        success: function (data) {
            var result = $(data.parse.text["*"]).find("p").first();
            callback(result);
        }
    });
}


