export interface ResumeAnalysisResponse {
  atsScore: number;
  resumeMatch: number;
  formattingScore: number;

  keywordMatch: number;
  matchedKeywords: string[];
  missingKeywords: string[];

  directMatches: DirectMatch[];
  transferableMatches: TransferableMatch[];
  gaps: Gap[];
  humanization: Humanization;
  atsTailoring: AtsTailoring[];
  applicationRecommendation: ApplicationRecommendation;
  finalRecommendation: string;
}
export interface KeywordMatch {
  score: ScoreDetail;
  matchedKeywords: string[];
  missingKeywords: string[];
}

export interface ScoreDetail {
  value: number;
  explanation: string;
}
export interface DirectMatch {
  jdRequirement: string;
  resumeEvidence: string;
  matchType: string;
}

export interface TransferableMatch {
  jdRequirement: string;
  resumeEvidence: string;
  reasoning: string;
  matchType: string;
}

export interface Gap {
  requirement: string;
  requirementType: string;
  gapType: string;
  severity: string;
  preparationSuggestion: string;
}

export interface Humanization {
  flaggedSentences: FlaggedSentence[];
}

export interface FlaggedSentence {
  original: string;
  action: string;
  rewritten: string;
  reason: string;
}

export interface AtsTailoring {
  jdRequirement: string;
  status: string;
  resumeEvidence: string;
  resumeSection: string;
  recommendation: string;
  suggestedTerminology: string[];
}

export interface ApplicationRecommendation {
  decision: string;
  confidence: string;
  reasoning: string;
  preparationPlan: string[];
}
