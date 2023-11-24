namespace TcFairplay

type CourtNo =
    | Court1
    | Court2
    | Court3

module GotCourtsData =
    let clubId = ClubId 53223

    let courtToId = function
        | Court1 -> CourtId 8153
        | Court2 -> CourtId 8154
        | Court3 -> CourtId 8155

    let idToCourt = function
        | CourtId 8153 -> Court1
        | CourtId 8154 -> Court2
        | CourtId 8155 -> Court3
        | CourtId id -> failwithf "Unknown court id '%d." id
