interface Props {
  options: string[];
  onAnswer: (answer: string) => void;
}

export default function MultiChoiceOptions({ options, onAnswer }: Props) {
  return (
    <div className="space-y-3">
      <p className="text-slate-500">Who said this?</p>
      {options.map((opt) => (
        <button
          key={opt}
          onClick={() => onAnswer(opt)}
          className="w-full py-3 px-5 rounded-xl bg-slate-50 hover:bg-indigo-50 border border-slate-200 hover:border-indigo-300 text-left text-slate-700 font-medium transition-colors cursor-pointer"
        >
          {opt}
        </button>
      ))}
    </div>
  );
}
