"""Arabic contextual shaping + minimal bidi, for baking text into textures.

Pillow here has no libraqm/harfbuzz, and Unity's legacy text stack does not
shape Arabic either.  This module is the single source of truth for the
shaping tables: it bakes Arabic into the sign/facade atlases *and* emits the
matching C# table used at runtime (see emit_csharp()).

Scope (deliberate):
  * joining classes + Unicode Arabic Presentation Forms-B
  * lam-alef ligatures
  * tatweel / harakat handling (diacritics are kept attached, not reordered)
  * run-level reordering: RTL runs reversed, LTR runs (latin, digits) kept
"""

JOIN_NONE = 0
JOIN_RIGHT = 1  # joins only to the preceding (right-hand) letter
JOIN_DUAL = 2   # joins on both sides

# codepoint -> (isolated, final, initial, medial, joining)
FORMS = {
    0x0621: (0xFE80, None, None, None, JOIN_NONE),      # hamza
    0x0622: (0xFE81, 0xFE82, None, None, JOIN_RIGHT),   # alef madda
    0x0623: (0xFE83, 0xFE84, None, None, JOIN_RIGHT),   # alef hamza above
    0x0624: (0xFE85, 0xFE86, None, None, JOIN_RIGHT),   # waw hamza
    0x0625: (0xFE87, 0xFE88, None, None, JOIN_RIGHT),   # alef hamza below
    0x0626: (0xFE89, 0xFE8A, 0xFE8B, 0xFE8C, JOIN_DUAL),  # yeh hamza
    0x0627: (0xFE8D, 0xFE8E, None, None, JOIN_RIGHT),   # alef
    0x0628: (0xFE8F, 0xFE90, 0xFE91, 0xFE92, JOIN_DUAL),  # beh
    0x0629: (0xFE93, 0xFE94, None, None, JOIN_RIGHT),   # teh marbuta
    0x062A: (0xFE95, 0xFE96, 0xFE97, 0xFE98, JOIN_DUAL),  # teh
    0x062B: (0xFE99, 0xFE9A, 0xFE9B, 0xFE9C, JOIN_DUAL),  # theh
    0x062C: (0xFE9D, 0xFE9E, 0xFE9F, 0xFEA0, JOIN_DUAL),  # jeem
    0x062D: (0xFEA1, 0xFEA2, 0xFEA3, 0xFEA4, JOIN_DUAL),  # hah
    0x062E: (0xFEA5, 0xFEA6, 0xFEA7, 0xFEA8, JOIN_DUAL),  # khah
    0x062F: (0xFEA9, 0xFEAA, None, None, JOIN_RIGHT),   # dal
    0x0630: (0xFEAB, 0xFEAC, None, None, JOIN_RIGHT),   # thal
    0x0631: (0xFEAD, 0xFEAE, None, None, JOIN_RIGHT),   # reh
    0x0632: (0xFEAF, 0xFEB0, None, None, JOIN_RIGHT),   # zain
    0x0633: (0xFEB1, 0xFEB2, 0xFEB3, 0xFEB4, JOIN_DUAL),  # seen
    0x0634: (0xFEB5, 0xFEB6, 0xFEB7, 0xFEB8, JOIN_DUAL),  # sheen
    0x0635: (0xFEB9, 0xFEBA, 0xFEBB, 0xFEBC, JOIN_DUAL),  # sad
    0x0636: (0xFEBD, 0xFEBE, 0xFEBF, 0xFEC0, JOIN_DUAL),  # dad
    0x0637: (0xFEC1, 0xFEC2, 0xFEC3, 0xFEC4, JOIN_DUAL),  # tah
    0x0638: (0xFEC5, 0xFEC6, 0xFEC7, 0xFEC8, JOIN_DUAL),  # zah
    0x0639: (0xFEC9, 0xFECA, 0xFECB, 0xFECC, JOIN_DUAL),  # ain
    0x063A: (0xFECD, 0xFECE, 0xFECF, 0xFED0, JOIN_DUAL),  # ghain
    0x0640: (0x0640, 0x0640, 0x0640, 0x0640, JOIN_DUAL),  # tatweel
    0x0641: (0xFED1, 0xFED2, 0xFED3, 0xFED4, JOIN_DUAL),  # feh
    0x0642: (0xFED5, 0xFED6, 0xFED7, 0xFED8, JOIN_DUAL),  # qaf
    0x0643: (0xFED9, 0xFEDA, 0xFEDB, 0xFEDC, JOIN_DUAL),  # kaf
    0x0644: (0xFEDD, 0xFEDE, 0xFEDF, 0xFEE0, JOIN_DUAL),  # lam
    0x0645: (0xFEE1, 0xFEE2, 0xFEE3, 0xFEE4, JOIN_DUAL),  # meem
    0x0646: (0xFEE5, 0xFEE6, 0xFEE7, 0xFEE8, JOIN_DUAL),  # noon
    0x0647: (0xFEE9, 0xFEEA, 0xFEEB, 0xFEEC, JOIN_DUAL),  # heh
    0x0648: (0xFEED, 0xFEEE, None, None, JOIN_RIGHT),   # waw
    0x0649: (0xFEEF, 0xFEF0, None, None, JOIN_RIGHT),   # alef maksura
    0x064A: (0xFEF1, 0xFEF2, 0xFEF3, 0xFEF4, JOIN_DUAL),  # yeh
    # Maghrebi additions used in Algeria
    0x0671: (0xFB50, 0xFB51, None, None, JOIN_RIGHT),   # alef wasla
    0x06A4: (0xFB6E, 0xFB6F, 0xFB70, 0xFB71, JOIN_DUAL),  # veh  (ڤ)
    0x06A9: (0xFB8E, 0xFB8F, 0xFB90, 0xFB91, JOIN_DUAL),  # keheh (ک)
    0x06AF: (0xFB92, 0xFB93, 0xFB94, 0xFB95, JOIN_DUAL),  # gaf  (گ)
}

# lam + alef-variant ligatures: (isolated, final)
LAM_ALEF = {
    0x0622: (0xFEF5, 0xFEF6),
    0x0623: (0xFEF7, 0xFEF8),
    0x0625: (0xFEF9, 0xFEFA),
    0x0627: (0xFEFB, 0xFEFC),
}

TASHKEEL = set(range(0x064B, 0x0660)) | {0x0670, 0x06D6, 0x06D7, 0x06D8,
                                         0x06D9, 0x06DA, 0x06DB, 0x06DC,
                                         0x06DF, 0x06E0, 0x06E1, 0x06E2,
                                         0x06E3, 0x06E4, 0x06E7, 0x06E8,
                                         0x06EA, 0x06EB, 0x06EC, 0x06ED}

ARABIC_RANGES = ((0x0600, 0x06FF), (0x0750, 0x077F), (0x08A0, 0x08FF),
                 (0xFB50, 0xFDFF), (0xFE70, 0xFEFF))


def is_arabic(cp):
    return any(a <= cp <= b for a, b in ARABIC_RANGES)


def is_letter(cp):
    """True for codepoints that take part in joining."""
    return cp in FORMS


def _joins_forward(cp):
    """Can this letter connect to the letter that follows it (logically)?"""
    f = FORMS.get(cp)
    return f is not None and f[4] == JOIN_DUAL


def _joins_backward(cp):
    """Can this letter accept a connection from the preceding letter?"""
    f = FORMS.get(cp)
    return f is not None and f[4] != JOIN_NONE


def shape_logical(text):
    """Map Arabic letters to their contextual presentation forms.

    Returns a list of codepoints in *logical* order, with ligatures applied
    and diacritics preserved where they were.
    """
    out = []
    i = 0
    n = len(text)
    while i < n:
        cp = ord(text[i])

        # lam + alef ligature
        if cp == 0x0644 and i + 1 < n and ord(text[i + 1]) in LAM_ALEF:
            nxt = ord(text[i + 1])
            prev_cp = out[-1] if out else None
            joined_prev = prev_cp is not None and _joins_forward(prev_cp)
            iso, fin = LAM_ALEF[nxt]
            out.append(fin if joined_prev else iso)
            i += 2
            continue

        if cp in TASHKEEL:
            out.append(cp)
            i += 1
            continue

        form = FORMS.get(cp)
        if form is None:
            out.append(cp)
            i += 1
            continue

        # find the nearest following joining letter (skip diacritics)
        j = i + 1
        while j < n and ord(text[j]) in TASHKEEL:
            j += 1
        next_cp = ord(text[j]) if j < n else None

        # find the nearest preceding joining letter in the output
        prev_cp = None
        for k in range(len(out) - 1, -1, -1):
            if out[k] in TASHKEEL:
                continue
            prev_cp = out[k]
            break

        can_prev = prev_cp is not None and _joins_forward(prev_cp) and _joins_backward(cp)
        can_next = (next_cp is not None and form[4] == JOIN_DUAL
                    and _joins_backward(next_cp))

        iso, fin, ini, med = form[0], form[1], form[2], form[3]
        if can_prev and can_next and med is not None:
            out.append(med)
        elif can_next and ini is not None:
            out.append(ini)
        elif can_prev and fin is not None:
            out.append(fin)
        else:
            out.append(iso)
        i += 1

    return out


def _classify(cp):
    if cp in TASHKEEL:
        return "nsm"
    if is_arabic(cp):
        return "rtl"
    ch = chr(cp)
    if ch.isdigit():
        return "ltr"
    if ch.isalpha():
        return "ltr"
    if ch in " \t\u00a0":
        return "ws"
    return "neu"


def shape(text):
    """Shape + reorder for display in a left-to-right renderer.

    Returns a string whose codepoints are in visual left-to-right order, so a
    plain draw.text() call renders correct Arabic.
    """
    if not text:
        return text

    logical = shape_logical(text)
    kinds = [_classify(cp) for cp in logical]

    # group into runs
    runs = []
    for cp, k in zip(logical, kinds):
        if runs and runs[-1][0] == k:
            runs[-1][1].append(cp)
        else:
            runs.append((k, [cp]))

    # merge neutral runs into a neighbour: prefer the following run, else previous
    merged = []
    for k, cps in runs:
        if k in ("neu", "ws") and merged:
            merged[-1][1].extend(cps)
        else:
            merged.append([k, list(cps)])
    if merged and merged[0][0] in ("neu", "ws") and len(merged) > 1:
        merged[1][1][0:0] = merged[0][1]
        merged.pop(0)

    # visual order: reverse run order, keep LTR runs internally forward
    out = []
    for k, cps in reversed(merged):
        if k == "ltr":
            out.extend(cps)
        else:
            out.extend(reversed(cps))
    return "".join(chr(cp) for cp in out)


def emit_csharp(namespace="AlgerBusSim.Localization.Generated", class_name="ArabicShapingTable"):
    """Emit the same table as C# so runtime and baked art can never diverge."""
    L = []
    L.append("// <auto-generated>")
    L.append("// Generated by tools/arabic.py - do not edit by hand.")
    L.append("// Regenerate with: python tools/arabic.py --emit-csharp")
    L.append("// </auto-generated>")
    L.append("namespace " + namespace)
    L.append("{")
    L.append("    internal static class " + class_name)
    L.append("    {")
    L.append("        // row layout: [cp, isolated, final, initial, medial, joining]")
    L.append("        internal static readonly int[][] Forms =")
    L.append("        new int[][]")
    L.append("        {")
    for cp in sorted(FORMS):
        iso, fin, ini, med, join = FORMS[cp]
        def g(v):
            return str(v) if v is not None else "-1"
        L.append("            new int[] { " + str(cp) + ", " + g(iso) + ", " + g(fin) + ", " + g(ini) + ", " + g(med) + ", " + str(join) + " },")
    L.append("        };")
    L.append("        internal static readonly int[][] LamAlefRows =")
    L.append("        new int[][]")
    L.append("        {")
    for cp in sorted(LAM_ALEF):
        iso, fin = LAM_ALEF[cp]
        L.append("            new int[] { " + str(cp) + ", " + str(iso) + ", " + str(fin) + " },")
    L.append("        };")
    L.append("        internal static int[] FormsLookup(int cp, out int isolated, out int final)")
    L.append("        {")
    L.append("            foreach (var row in Forms)")
    L.append("            {")
    L.append("                if (row[0] == cp) { isolated = row[1]; final = row[2]; return row; }")
    L.append("            }")
    L.append("            isolated = 0; final = 0; return null;")
    L.append("        }")
    L.append("        internal static int LamAlefLookupRow(int cp, out int isolated, out int final)")
    L.append("        {")
    L.append("            foreach (var row in LamAlefRows)")
    L.append("            {")
    L.append("                if (row[0] == cp) { isolated = row[1]; final = row[2]; return 1; }")
    L.append("            }")
    L.append("            isolated = 0; final = 0; return 0;")
    L.append("        }")
    L.append("    }")
    L.append("}")
    return "\n".join(L) + "\n"



if __name__ == "__main__":
    import os
    import sys
    sys.stdout.reconfigure(encoding="utf-8")
    for s in ["الجزائر", "باب الوادي", "شارع ديدوش مراد", "محطة الحافلات",
              "الجزائر الوسطى", "قالمة", "سطيف", "تيزي وزو"]:
        print(s, "->", shape(s))
    out = emit_csharp()
    dest = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
                        "Assets", "AlgerBusSim", "Runtime", "Localization", "Generated")
    os.makedirs(dest, exist_ok=True)
    with open(os.path.join(dest, "ArabicShapingTable.cs"), "w", encoding="utf-8") as fh:
        fh.write(out)
    print("wrote", os.path.join(dest, "ArabicShapingTable.cs"))
