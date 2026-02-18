interface Props {
  displayedAuthor: string;
  onAnswer: (answer: string) => void;
}

export default function BinaryOptions({ displayedAuthor, onAnswer }: Props) {
  return (
    <div className="space-y-4">
      <p className="text-slate-500">
        Was this quote said by{" "}
        <span className="text-slate-800 font-semibold">{displayedAuthor}</span>?
      </p>
      <div className="flex gap-4">
        <button
          onClick={() => onAnswer(displayedAuthor)}
          className="flex-1 py-3 rounded-xl bg-green-500 hover:bg-green-400 text-white font-semibold transition-colors cursor-pointer shadow-sm"
        >
          Yes
        </button>
        <button
          onClick={() => onAnswer("__wrong__")}
          className="flex-1 py-3 rounded-xl bg-red-500 hover:bg-red-400 text-white font-semibold transition-colors cursor-pointer shadow-sm"
        >
          No
        </button>
      </div>
    </div>
  );
}
