﻿namespace IronHive.Providers.GoogleAI.EmbedContent.Models;

internal enum EmbedTaskType
{
    // Unset value, which will default to one of the other enum values
    TASK_TYPE_UNSPECIFIED,

    // Specifies the given text is a query in a search/retrieval setting.
    RETRIEVAL_QUERY,

    // Specifies the given text is a document from the corpus being searched.
    RETRIEVAL_DOCUMENT,

    // Specifies the given text will be used for STS.
    SEMANTIC_SIMILARITY,

    // Specifies that the given text will be classified.
    CLASSIFICATION,

    // Specifies that the embeddings will be used for clustering.
    CLUSTERING,

    // Specifies that the given text will be used for question answering.
    QUESTION_ANSWERING,

    // Specifies that the given text will be used for fact verification.
    FACT_VERIFICATION,

    // Specifies that the given text will be used for code retrieval.
    CODE_RETRIEVAL_QUERY
}
