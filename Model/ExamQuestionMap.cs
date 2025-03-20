using CsvHelper.Configuration;

namespace QAssessment_project.Model
{
    public sealed class ExamQuestionMap : ClassMap<ExamQuestionCsv>
    {
        public ExamQuestionMap()
        {
            Map(m => m.Question).Name("Question");
            Map(m => m.OptionA).Name("Option A");
            Map(m => m.OptionB).Name("Option B");
            Map(m => m.OptionC).Name("Option C");
            Map(m => m.OptionD).Name("Option D");
            Map(m => m.Answer).Name("Answer");
        }
    }
}
